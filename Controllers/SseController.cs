using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Options;
using QuizForge.Realtime;

namespace QuizForge.Controllers;

[ApiController]
[Route("sse")]
public class SseController(
    ISseHub sseHub,
    IOptions<SseOptions> sseOptions,
    ILogger<SseController> logger
) : ControllerBase
{
    private readonly ISseHub _sseHub = sseHub;
    private readonly ILogger<SseController> _logger = logger;
    private readonly SseOptions _sseOptions = sseOptions.Value;

    [Authorize]
    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var streamKey = SseHub.GetUserStreamKey(userId);
        var lastEventId = TryGetLastEventId();

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers[HeaderNames.CacheControl] = "no-cache, no-transform";
        Response.Headers[HeaderNames.Connection] = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        await Response.StartAsync(cancellationToken);

        await using var subscription = _sseHub.Subscribe(streamKey, lastEventId);
        await WriteRetryAsync(Response, _sseOptions.RetryMilliseconds, cancellationToken);
        await WriteCommentAsync(
            Response,
            $"connected={subscription.ConnectionId}",
            cancellationToken
        );

        var keepAlive = TimeSpan.FromSeconds(_sseOptions.KeepAliveSeconds);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var waitToReadTask = subscription.Reader.WaitToReadAsync(cancellationToken).AsTask();
                var keepAliveTask = Task.Delay(keepAlive, cancellationToken);
                var completedTask = await Task.WhenAny(waitToReadTask, keepAliveTask);

                if (completedTask == keepAliveTask)
                {
                    await WriteCommentAsync(Response, "keep-alive", cancellationToken);
                    continue;
                }

                if (!await waitToReadTask)
                    break;

                while (subscription.Reader.TryRead(out var envelope))
                    await WriteEventAsync(Response, envelope, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (IOException)
        {
        }
        finally
        {
            _logger.LogInformation(
                "SSE stream closed for user {UserId}, Connection={ConnectionId}",
                userId,
                subscription.ConnectionId
            );
        }
    }

    [Authorize]
    [HttpPost("self-test")]
    public async Task<IActionResult> PublishSelfTest(
        PublishSseEventRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        object payload = dto.Data is null ? new { ok = true } : dto.Data.Value;

        var delivered = await _sseHub.PublishToUserAsync(
            userId,
            dto.EventName,
            payload,
            cancellationToken
        );

        var response = ApiResponseDto<object>.Success(new
        {
            delivered
        }, "Publish SSE thành công");
        return Ok(response);
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!long.TryParse(sub, out var userId))
            throw new UnauthorizedException("Access token không hợp lệ");

        return userId;
    }

    private long? TryGetLastEventId()
    {
        if (!Request.Headers.TryGetValue("Last-Event-ID", out var raw))
            return null;

        var rawValue = raw.ToString();
        if (!long.TryParse(rawValue, out var lastEventId))
        {
            _logger.LogWarning("Invalid Last-Event-ID header value: {HeaderValue}", rawValue);
            return null;
        }

        return lastEventId;
    }

    private static async Task WriteRetryAsync(
        HttpResponse response,
        int retryMilliseconds,
        CancellationToken cancellationToken
    )
    {
        var frame = $"retry: {retryMilliseconds}\n\n";
        await response.WriteAsync(frame, cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteCommentAsync(
        HttpResponse response,
        string comment,
        CancellationToken cancellationToken
    )
    {
        var frame = $": {comment}\n\n";
        await response.WriteAsync(frame, cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteEventAsync(
        HttpResponse response,
        SseEventEnvelope envelope,
        CancellationToken cancellationToken
    )
    {
        var sb = new StringBuilder();
        sb.Append("id: ").Append(envelope.Id).Append('\n');
        sb.Append("event: ").Append(envelope.EventName).Append('\n');

        var normalizedData = envelope.Data.Replace("\r\n", "\n").Replace("\r", "\n");
        foreach (var line in normalizedData.Split('\n'))
            sb.Append("data: ").Append(line).Append('\n');

        sb.Append('\n');

        await response.WriteAsync(sb.ToString(), cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
