using System.Net.Http.Json;
using QuizForge.DTOs;
using QuizForge.Models;
using QuizForge.Providers;
using QuizForge.Repositories;

namespace QuizForge.Consumers;

public class DocumentConsumer(
    IMessageQueueProvider messageQueueProvider,
    IHttpClientFactory httpClientFactory,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<DocumentConsumer> logger
) : BackgroundService
{
    private readonly IMessageQueueProvider _messageQueueProvider = messageQueueProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<DocumentConsumer> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _messageQueueProvider.ConsumeAsync(HandleMessageAsync, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer crashed, retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(ExtractDocumentMessageDto message, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new ParseDocumentRequestDto(
                FileKey: message.FileKey,
                Backend: "pipeline",
                ParseMethod: "auto"
            );

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.PostAsJsonAsync(
                requestUri: "http://127.0.0.1:8000/gcs_parse",
                value: requestBody,
                cancellationToken: cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Parse service failed for file '{FileKey}'. StatusCode={StatusCode}, Body={Body}",
                    message.FileKey,
                    (int)response.StatusCode,
                    errorBody
                );
                return;
            }

            var parseResponse = await response.Content.ReadFromJsonAsync<ParseDocumentResponseDto>(
                cancellationToken: cancellationToken
            );

            if (parseResponse is null)
            {
                _logger.LogError(
                    "Parse service returned empty response body for file '{FileKey}'",
                    message.FileKey
                );
                return;
            }

            if (string.Equals(parseResponse.Status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                if (parseResponse.Result is null)
                {
                    _logger.LogError(
                        "Parse service returned status completed but missing result for file '{FileKey}'",
                        message.FileKey
                    );
                    return;
                }

                await SaveExtractionAsync(message.HashSha256, parseResponse, cancellationToken);
            }

            _logger.LogInformation(
                "Parsed file '{FileKey}'. Status={Status}, TaskId={TaskId}, Backend={Backend}, Error={Error}",
                message.FileKey,
                parseResponse.Status,
                parseResponse.TaskId,
                parseResponse.Backend,
                parseResponse.Error
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process parse flow for file '{FileKey}'", message.FileKey);
        }
    }

    private async Task SaveExtractionAsync(
        string hashSha256,
        ParseDocumentResponseDto parseResponse,
        CancellationToken cancellationToken
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var extractionRepository = scope.ServiceProvider.GetRequiredService<IExtractionRepository>();

        var existedExtraction = await extractionRepository.FindByHashSha256(hashSha256, cancellationToken);
        if (existedExtraction is not null)
            return;

        var extraction = new Extraction
        {
            HashSha256 = hashSha256,
            Version = parseResponse.Version,
            RawJson = parseResponse.Result!.Data.GetRawText()
        };

        await extractionRepository.CreateAsync(extraction, cancellationToken);

        _logger.LogInformation("Created extraction for hash '{HashSha256}'", hashSha256);
    }
}
