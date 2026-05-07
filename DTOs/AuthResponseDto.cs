namespace QuizForge.DTOs;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int AccessExpiresIn,
    int RefreshExpiresIn
);