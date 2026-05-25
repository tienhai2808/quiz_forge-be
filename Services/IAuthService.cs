using QuizForge.DTOs;

namespace QuizForge.Services;

public interface IAuthService
{
    Task<(UserResponseDto, string, string)> GoogleLoginAsync(GoogleLoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UserInfoAsync(long userId, CancellationToken cancellationToken = default);
    Task LogoutAsync(long userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<(string, string)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
