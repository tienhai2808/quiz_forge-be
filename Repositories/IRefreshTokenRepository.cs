using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<int> DeleteActiveByTokenAndUserIdAsync(
        string token,
        long userId,
        CancellationToken cancellationToken = default
    );
    Task<int> RotateActiveAsync(
        string currentToken,
        string newToken,
        DateTime newExpiresAtUtc,
        CancellationToken cancellationToken = default
    );
}
