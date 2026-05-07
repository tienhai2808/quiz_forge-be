using QuizForge.DTOs;

namespace QuizForge.Providers;

public interface IGoogleProvider
{
    Task<GoogleTokenResponseDto> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default
    );

    Task<GoogleUserInfoDto> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
}
