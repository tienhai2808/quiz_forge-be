using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Options;

namespace QuizForge.Providers;

public class GoogleOAuthProvider(
    HttpClient httpClient,
    IOptions<GoogleOAuthOptions> googleOAuthOptions
) : IOAuthProvider
{
    private readonly GoogleOAuthOptions _googleOAuthOptions = googleOAuthOptions.Value;
    private readonly HttpClient _httpClient = httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OAuthTokenDto> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken
    )
    {
        ValidateGoogleConfig(redirectUri);

        var formData = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _googleOAuthOptions.ClientId,
            ["client_secret"] = _googleOAuthOptions.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _googleOAuthOptions.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(formData)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedException("Google authorization code không hợp lệ hoặc đã hết hạn");

        var tokenResponse = JsonSerializer.Deserialize<OAuthTokenDto>(responseContent, JsonOptions);
        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            throw new ExternalServiceException("Không thể đọc token response từ Google");

        return tokenResponse;
    }

    public async Task<OAuthUserDto> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _googleOAuthOptions.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedException("Google access token không hợp lệ");

        var userInfo = JsonSerializer.Deserialize<OAuthUserDto>(responseContent, JsonOptions);
        if (userInfo is null || string.IsNullOrWhiteSpace(userInfo.Sub))
            throw new ExternalServiceException("Không thể đọc thông tin người dùng từ Google");

        return userInfo;
    }

    private void ValidateGoogleConfig(string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(_googleOAuthOptions.ClientId) ||
            string.IsNullOrWhiteSpace(_googleOAuthOptions.ClientSecret))
            throw new InternalServerException("Thiếu cấu hình GoogleAuth ClientId/ClientSecret");

        if (!_googleOAuthOptions.AllowedRedirectUris.Contains(redirectUri))
            throw new BadRequestException("Uri chuyển hướng không hợp lệ");
    }
}
