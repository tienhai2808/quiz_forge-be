using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using AppConstants = QuizForge.Common.Constants;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QuizForge.Providers;

public class RabbitMqProvider(
    IConnection connection,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    ILogger<RabbitMqProvider> logger
) : IMessageQueueProvider
{
    private readonly IConnection _connection = connection;
    private readonly RabbitMqOptions _rabbitMqOptions = rabbitMqOptions.Value;
    private readonly ILogger<RabbitMqProvider> _logger = logger;

    public async Task PublishAsync(ExtractDocumentMessageDto message, CancellationToken cancellationToken = default)
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: AppConstants.ExchangeDocument,
            routingKey: AppConstants.RoutingKeyExtractDocument,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Published message to exchange '{Exchange}' with routing key '{RoutingKey}'",
            AppConstants.ExchangeDocument,
            AppConstants.RoutingKeyExtractDocument
        );
    }

    public async Task ConsumeAsync(
        Func<ExtractDocumentMessageDto, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default
    )
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)_rabbitMqOptions.ConsumerPrefetchCount,
            global: false,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var payload = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var message = JsonSerializer.Deserialize<ExtractDocumentMessageDto>(payload)
                ?? throw new BadRequestException("Message payload is invalid");

            try
            {
                await messageHandler(message, cancellationToken);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception ex)
            {
                var rejectedCount = GetRejectedCount(eventArgs);
                if (rejectedCount >= _rabbitMqOptions.MaxRetryAttempts)
                {
                    await PublishToDlqAsync(channel, eventArgs, ex, cancellationToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);

                    _logger.LogError(
                        ex,
                        "Message moved to DLQ after {Retries} retries",
                        rejectedCount
                    );
                }
                else
                {
                    await channel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: cancellationToken
                    );

                    _logger.LogWarning(
                        ex,
                        "Message processing failed. Sent to retry queue. Retry count: {RetryCount}",
                        rejectedCount + 1
                    );
                }
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: AppConstants.QueueNameExtractDocument,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("RabbitMQ consumer started with tag '{ConsumerTag}'", consumerTag);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            await channel.BasicCancelAsync(consumerTag, cancellationToken: CancellationToken.None);
        }
    }

    private static int GetRejectedCount(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs.BasicProperties.Headers is not { } headers)
            return 0;

        if (!headers.TryGetValue("x-death", out var xDeath) || xDeath is not IList deaths)
            return 0;

        var rejectedCount = 0;
        foreach (var deathEntry in deaths)
        {
            if (deathEntry is not IDictionary deathInfo)
                continue;

            var queueName = deathInfo["queue"]?.ToString();
            var reason = deathInfo["reason"]?.ToString();
            if (!string.Equals(queueName, AppConstants.QueueNameExtractDocument, StringComparison.Ordinal))
                continue;

            if (!string.Equals(reason, "rejected", StringComparison.OrdinalIgnoreCase))
                continue;

            if (deathInfo["count"] is long countAsLong)
                rejectedCount += (int)countAsLong;
            else if (int.TryParse(deathInfo["count"]?.ToString(), out var countAsInt))
                rejectedCount += countAsInt;
        }

        return rejectedCount;
    }

    private static async Task PublishToDlqAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var headers = new Dictionary<string, object?>
        {
            ["x-original-exchange"] = eventArgs.Exchange,
            ["x-original-routing-key"] = eventArgs.RoutingKey,
            ["x-error"] = exception.Message,
            ["x-failed-at-utc"] = DateTimeOffset.UtcNow.ToString("O")
        };

        var properties = new BasicProperties
        {
            Persistent = true,
            Headers = headers,
            MessageId = eventArgs.BasicProperties.MessageId,
            CorrelationId = eventArgs.BasicProperties.CorrelationId,
            ContentType = eventArgs.BasicProperties.ContentType
        };

        await channel.BasicPublishAsync(
            exchange: AppConstants.ExchangeDocumentDlq,
            routingKey: AppConstants.RoutingKeyExtractDocumentDlq,
            mandatory: false,
            basicProperties: properties,
            body: eventArgs.Body,
            cancellationToken: cancellationToken
        );
    }
}
