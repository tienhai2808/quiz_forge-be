using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public required string SigningKey { get; init; }
    public string Issuer { get; init; } = "QuizForge";
    public string Audience { get; init; } = "QuizForgeClient";

    [Range(1, int.MaxValue)]
    public int AccessTokenExpiresMinutes { get; init; } = 30;
}
