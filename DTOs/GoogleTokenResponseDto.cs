using System.Text.Json.Serialization;

namespace QuizForge.DTOs;

public sealed record GoogleTokenResponseDto(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("id_token")] string? IdToken
);
