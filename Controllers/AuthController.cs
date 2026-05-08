using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizForge.Constants;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Services;

namespace QuizForge.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    IConfiguration configuration,
    IAuthService authService, 
    IWebHostEnvironment environment
) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly int _accessExpiresInMinutes = configuration.GetValue("Jwt:AccessTokenExpiresMinutes", 30);
    private readonly int _refreshExpiresInDays = configuration.GetValue("Token:RefreshTokenExpiresDays", 7);

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(
        GoogleLoginRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var (userRes, accessToken, refreshToken) = await _authService.GoogleLoginAsync(dto, cancellationToken);
        WriteAuthCookies(accessToken, refreshToken);

        var response = ApiResponseDto<UserResponseDto>.Success(userRes, "Đăng nhập thành công");
        return Ok(response);
    }

    [Authorize]
    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo(CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
            throw new UnauthorizedException("Access token không hợp lệ");

        var userRes = await _authService.UserInfoAsync(userId, cancellationToken);
        var response = ApiResponseDto<UserResponseDto>.Success(userRes);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();

        if (Request.Cookies.TryGetValue(AuthConstants.RefreshToken, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(userId, refreshToken, cancellationToken);
        }

        ClearAuthCookies();

        var response = ApiResponseDto<object>.Success(null, "Đăng xuất thành công");
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        CancellationToken cancellationToken
    )
    {
        if (!Request.Cookies.TryGetValue(AuthConstants.RefreshToken, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("Refresh token không hợp lệ");
        }

        try
        {
            var (newAccessToken, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
            WriteAuthCookies(newAccessToken, newRefreshToken);
        }
        catch (UnauthorizedException)
        {
            ClearAuthCookies();
            throw;
        }

        var response = ApiResponseDto<object>.Success(null);
        return Ok(response);
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
            throw new UnauthorizedException("Access token không hợp lệ");

        return userId;
    }

    private void WriteAuthCookies(string accessToken, string refreshToken)
    {
        var secure = !_environment.IsDevelopment();
        var nowUtc = DateTimeOffset.UtcNow;
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = nowUtc.AddMinutes(_accessExpiresInMinutes)
        };
        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = nowUtc.AddDays(_refreshExpiresInDays)
        };

        Response.Cookies.Append(AuthConstants.AccessToken, accessToken, accessCookieOptions);
        Response.Cookies.Append(AuthConstants.RefreshToken, refreshToken, refreshCookieOptions);
    }

    private void ClearAuthCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        Response.Cookies.Delete(AuthConstants.AccessToken, cookieOptions);
        Response.Cookies.Delete(AuthConstants.RefreshToken, cookieOptions);
    }
}
