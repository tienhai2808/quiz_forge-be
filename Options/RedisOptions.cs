using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; init; }
}
