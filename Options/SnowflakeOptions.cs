using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record SnowflakeOptions
{
    public const string SectionName = "Snowflake";

    [Range(0, 1023)]
    public int GeneratorId { get; init; } = 0;

    public DateTime Epoch { get; init; } = new(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc);
}
