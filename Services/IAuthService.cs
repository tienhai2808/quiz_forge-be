using QuizForge.DTOs;

namespace QuizForge.Services;

public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UserInfoAsync(long userId, CancellationToken cancellationToken = default);
    Task LogoutAsync(long userId, LogoutRequestDto dto, CancellationToken cancellationToken = default);
}
