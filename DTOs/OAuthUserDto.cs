using System.Text.Json.Serialization;

namespace QuizForge.DTOs;

public sealed record OAuthUserDto(
    [property: JsonPropertyName("sub")] string Sub,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("picture")] string? Picture
);
