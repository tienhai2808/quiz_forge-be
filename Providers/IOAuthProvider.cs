using QuizForge.DTOs;

namespace QuizForge.Providers;

public interface IOAuthProvider
{
    Task<OAuthTokenDto> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default
    );

    Task<OAuthUserDto> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
}
