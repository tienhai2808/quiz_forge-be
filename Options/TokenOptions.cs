using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record TokenOptions
{
    public const string SectionName = "Token";

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpiresDays { get; init; } = 7;
}
