namespace QuizForge.DTOs;

public sealed record UserResponseDto(
    string Id,
    string Email,
    string Name,
    string? AvatarUrl,
    DateTime CreatedAt
);