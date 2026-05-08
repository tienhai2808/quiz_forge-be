using QuizForge.Data;
using QuizForge.Models;
using Microsoft.EntityFrameworkCore;

namespace QuizForge.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> FindByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    )
        => _db.RefreshTokens.FirstOrDefaultAsync(
            x => x.Token == token,
            cancellationToken
        );

    public Task<int> DeleteActiveByTokenAndUserIdAsync(
        string token,
        long userId,
        CancellationToken cancellationToken = default
    )
        => _db.RefreshTokens
            .Where(x => x.Token == token && x.UserId == userId && x.ExpiresAt >= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);

    public Task<int> RotateActiveAsync(
        string currentToken,
        string newToken,
        DateTime newExpiresAtUtc,
        CancellationToken cancellationToken = default
    )
        => _db.RefreshTokens
            .Where(x => x.Token == currentToken && x.ExpiresAt >= DateTime.UtcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Token, newToken)
                    .SetProperty(x => x.ExpiresAt, newExpiresAtUtc),
                cancellationToken
            );
}
