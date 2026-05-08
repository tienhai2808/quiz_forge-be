using System.Security.Cryptography;
using System.Text;
using IdGen;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Mappers;
using QuizForge.Models;
using QuizForge.Providers;
using QuizForge.Repositories;

namespace QuizForge.Services;

public class AuthService(
    IConfiguration configuration,
    ILogger<AuthService> logger,
    IIdGenerator<long> idGenerator,
    IGoogleProvider googleProvider,
    IJwtProvider jwtProvider,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository
) : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly IGoogleProvider _googleProvider = googleProvider;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly int _refreshExpiresInDays = configuration.GetValue("Token:RefreshTokenExpiresDays", 7);

    public async Task<(UserResponseDto, string, string)> GoogleLoginAsync(
        GoogleLoginRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResponse = await _googleProvider.ExchangeCodeAsync(
            dto.Code,
            dto.RedirectUri,
            cancellationToken
        );

        var userResponse = await _googleProvider.GetUserInfoAsync(
            tokenResponse.AccessToken,
            cancellationToken
        );

        var user = await _userRepository.FindByGoogleIdAsync(userResponse.Sub, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = _idGenerator.CreateId(),
                Name = userResponse.Name,
                GoogleId = userResponse.Sub,
                Email = userResponse.Email,
                AvatarUrl = userResponse.Picture
            };
            await _userRepository.CreateAsync(user, cancellationToken);
        }
        else
        {
            user.Name = userResponse.Name;
            user.AvatarUrl = userResponse.Picture;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user.Id);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenRecord = new RefreshToken
        {
            Token = Sha256Hash(refreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshExpiresInDays)
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenRecord, cancellationToken);

        return (user.ToUserResponse(), accessToken, refreshToken);
    }

    public async Task<UserResponseDto> UserInfoAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng");

        return user.ToUserResponse();
    }

    public async Task LogoutAsync(
        long userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var refreshTokenHash = Sha256Hash(refreshToken);
        var deletedRows = await _refreshTokenRepository.DeleteActiveByTokenAndUserIdAsync(
            refreshTokenHash,
            userId,
            cancellationToken
        );
        _logger.LogDebug("Deleted {DeletedRows} refresh token rows for user {UserId}", deletedRows, userId);
    }

    public async Task<(string, string)> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var nowUtc = DateTime.UtcNow;
        var refreshTokenHash = Sha256Hash(refreshToken);
        var refreshTokenRecord = await _refreshTokenRepository.FindByTokenAsync(refreshTokenHash, cancellationToken) ??
            throw new UnauthorizedException("Refresh token không hợp lệ");
        if (refreshTokenRecord.ExpiresAt < nowUtc)
            throw new UnauthorizedException("Refresh token đã hết hạn");

        var user = await _userRepository.FindByIdAsync(refreshTokenRecord.UserId, cancellationToken) ?? 
            throw new UnauthorizedException("Refresh token không hợp lệ");
        
        var accessToken = _jwtProvider.GenerateAccessToken(user.Id);
        var newRefreshToken = GenerateRefreshToken();
        var rotatedRows = await _refreshTokenRepository.RotateActiveAsync(
            refreshTokenHash,
            Sha256Hash(newRefreshToken),
            nowUtc.AddDays(_refreshExpiresInDays),
            cancellationToken
        );
        if (rotatedRows == 0)
            throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã được sử dụng");

        return (accessToken, newRefreshToken);
    }

    private static string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

    private static string Sha256Hash(string str)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
