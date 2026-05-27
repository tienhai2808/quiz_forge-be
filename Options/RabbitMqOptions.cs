using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    [Required]
    public required string Host { get; init; }

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    public string VHost { get; init; } = "/";

    [Range(1, 300)]
    public int RequestedHeartbeatSeconds { get; init; } = 30;

    [Range(1, 300)]
    public int NetworkRecoveryIntervalSeconds { get; init; } = 5;

    [Range(100, 86400000)]
    public int RetryDelayMilliseconds { get; init; } = 10000;

    [Range(1, 100)]
    public int MaxRetryAttempts { get; init; } = 3;

    [Range(1, 1000)]
    public int ConsumerPrefetchCount { get; init; } = 10;

    public bool AutomaticRecoveryEnabled { get; init; } = true;
    public bool TopologyRecoveryEnabled { get; init; } = true;
    public string ClientProvidedName { get; init; } = "quiz-forge-backend";
}
