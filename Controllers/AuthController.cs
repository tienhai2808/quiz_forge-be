using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Services;

namespace QuizForge.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(
        GoogleLoginRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var authRes = await _authService.GoogleLoginAsync(dto, cancellationToken);
        var response = ApiResponseDto<AuthResponseDto>.Success(
            authRes,
            "Đăng nhập thành công"
        );
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
        var response = ApiResponseDto<UserResponseDto>.Success(userRes, "Lấy thông tin người dùng thành công");
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
            throw new UnauthorizedException("Access token không hợp lệ");

        await _authService.LogoutAsync(userId, dto, cancellationToken);

        var response = ApiResponseDto<object>.Success(null, "Đăng xuất thành công");
        return Ok(response);
    }
}
