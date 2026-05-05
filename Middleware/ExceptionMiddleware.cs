using QuizForge.DTOs;
using QuizForge.Exceptions;

namespace QuizForge.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            await WriteError(context, ex.Message, ex.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, "Lỗi máy chủ nội bộ", 500);
        }
    }

    private static async Task WriteError(HttpContext context, string message, int status)
    {
        var response = ApiResponseDto<object>.Fail(null, message);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }
}