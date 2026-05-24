using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record GoogleOAuthOptions
{
    public const string SectionName = "GoogleAuth";

    [Required]
    public required string ClientId { get; init; }
    [Required]
    public required string ClientSecret { get; init; }
    public string TokenEndpoint { get; init; } = "https://oauth2.googleapis.com/token";
    public string UserInfoEndpoint { get; init; } = "https://openidconnect.googleapis.com/v1/userinfo";

    [Required, MinLength(1)]
    public required List<string> AllowedRedirectUris { get; init; }
}
