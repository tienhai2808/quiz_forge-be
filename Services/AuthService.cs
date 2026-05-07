using System.Security.Cryptography;
using System.Text;
using IdGen;
using QuizForge.DTOs;
using QuizForge.Exceptions;
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
    private readonly int _accessExpiresInMinutes = configuration.GetValue("Jwt:AccessTokenExpiresMinutes", 30);
    private readonly int _refreshExpiresInDays = configuration.GetValue("Token:RefreshTokenExpiresDays", 7);

    public async Task<AuthResponseDto> GoogleLoginAsync(
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
        var refreshTokenHash = Sha256Hash(refreshToken);
        var refreshTokenRecord = new RefreshToken
        {
            Token = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshExpiresInDays)
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenRecord, cancellationToken);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessExpiresIn: _accessExpiresInMinutes * 60,
            RefreshExpiresIn: _refreshExpiresInDays * 24 * 60 * 60
        );
    }

    public async Task<UserResponseDto> UserInfoAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng");

        return new UserResponseDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Name: user.Name,
            AvatarUrl: user.AvatarUrl ?? string.Empty,
            CreatedAt: user.CreatedAt
        );
    }

    public async Task LogoutAsync(
        long userId,
        LogoutRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var refreshTokenHash = Sha256Hash(dto.RefreshToken);

        var refreshToken = await _refreshTokenRepository.FindByTokenAsync(refreshTokenHash, cancellationToken) ??
            throw new UnauthorizedException("Refresh token không hợp lệ");
        if (refreshToken.UserId != userId)
            throw new UnauthorizedException("Refresh token không hợp lệ");
        if (refreshToken.ExpiresAt < DateTime.Now)
            throw new UnauthorizedException("Refresh token đã hết hạn");
        
        await _refreshTokenRepository.DeleteAsync(refreshToken, cancellationToken);
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
