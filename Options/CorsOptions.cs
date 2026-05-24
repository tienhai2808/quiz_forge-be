using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record CorsOptions
{
    public const string SectionName = "Cors";

    [Required, MinLength(1)]
    public required List<string> AllowedOrigins { get; init; }
}
