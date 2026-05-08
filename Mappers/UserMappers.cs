using QuizForge.DTOs;
using QuizForge.Models;

namespace QuizForge.Mappers;

public static class UserMappers
{
    public static UserResponseDto ToUserResponse(this User user)
        => new(
            Id: user.Id.ToString(),
            Email: user.Email,
            Name: user.Name,
            AvatarUrl: user.AvatarUrl,
            CreatedAt: user.CreatedAt
        );
}