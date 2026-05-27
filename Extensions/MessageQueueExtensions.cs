using Microsoft.Extensions.Options;
using AppConstants = QuizForge.Common.Constants;
using QuizForge.Consumers;
using QuizForge.Options;
using QuizForge.Providers;
using RabbitMQ.Client;

namespace QuizForge.Extensions;

public static class MessageQueueExtensions
{
    public static IServiceCollection AddAppMessageQueue(this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("RabbitMqConnection");

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                VirtualHost = options.VHost,
                ClientProvidedName = options.ClientProvidedName,
                RequestedHeartbeat = TimeSpan.FromSeconds(options.RequestedHeartbeatSeconds),
                AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled,
                TopologyRecoveryEnabled = options.TopologyRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(options.NetworkRecoveryIntervalSeconds)
            };

            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            logger.LogInformation("RabbitMQ connected as '{ClientName}'", options.ClientProvidedName);
            return connection;
        });

        services.AddSingleton<IMessageQueueProvider, RabbitMqProvider>();
        services.AddHostedService<RabbitMqTopologyInitializer>();
        services.AddHostedService<DocumentConsumer>();

        return services;
    }

    private sealed class RabbitMqTopologyInitializer(
        IConnection connection,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ILogger<RabbitMqTopologyInitializer> logger
    ) : IHostedService
    {
        private readonly IConnection _connection = connection;
        private readonly RabbitMqOptions _rabbitMqOptions = rabbitMqOptions.Value;
        private readonly ILogger<RabbitMqTopologyInitializer> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: AppConstants.ExchangeDocument,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.ExchangeDeclareAsync(
                exchange: AppConstants.ExchangeDocumentRetry,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.ExchangeDeclareAsync(
                exchange: AppConstants.ExchangeDocumentDlq,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            var mainQueueArgs = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = AppConstants.ExchangeDocumentRetry,
                ["x-dead-letter-routing-key"] = AppConstants.RoutingKeyExtractDocumentRetry,
                ["x-queue-type"] = "quorum"
            };

            await channel.QueueDeclareAsync(
                queue: AppConstants.QueueNameExtractDocument,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: mainQueueArgs,
                cancellationToken: cancellationToken
            );

            var retryQueueArgs = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = _rabbitMqOptions.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = AppConstants.ExchangeDocument,
                ["x-dead-letter-routing-key"] = AppConstants.RoutingKeyExtractDocument,
                ["x-queue-type"] = "quorum"
            };

            await channel.QueueDeclareAsync(
                queue: AppConstants.QueueNameExtractDocumentRetry,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryQueueArgs,
                cancellationToken: cancellationToken
            );

            var dlqQueueArgs = new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum"
            };

            await channel.QueueDeclareAsync(
                queue: AppConstants.QueueNameExtractDocumentDlq,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: dlqQueueArgs,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: AppConstants.QueueNameExtractDocument,
                exchange: AppConstants.ExchangeDocument,
                routingKey: AppConstants.RoutingKeyExtractDocument,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: AppConstants.QueueNameExtractDocumentRetry,
                exchange: AppConstants.ExchangeDocumentRetry,
                routingKey: AppConstants.RoutingKeyExtractDocumentRetry,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: AppConstants.QueueNameExtractDocumentDlq,
                exchange: AppConstants.ExchangeDocumentDlq,
                routingKey: AppConstants.RoutingKeyExtractDocumentDlq,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "RabbitMQ topology initialized: main '{MainQueue}', retry '{RetryQueue}', dlq '{DlqQueue}'",
                AppConstants.QueueNameExtractDocument,
                AppConstants.QueueNameExtractDocumentRetry,
                AppConstants.QueueNameExtractDocumentDlq
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
