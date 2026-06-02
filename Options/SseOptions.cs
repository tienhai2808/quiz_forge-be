using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record SseOptions
{
    public const string SectionName = "Sse";

    [Range(5, 300)]
    public int KeepAliveSeconds { get; init; } = 15;

    [Range(500, 60000)]
    public int RetryMilliseconds { get; init; } = 3000;

    [Range(10, 10000)]
    public int ClientQueueCapacity { get; init; } = 200;

    [Range(10, 5000)]
    public int ReplayCapacityPerStream { get; init; } = 200;
}
