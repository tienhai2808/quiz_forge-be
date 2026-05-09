using System.Net.Http.Headers;
using System.Text.Json;
using QuizForge.DTOs;
using QuizForge.Exceptions;

namespace QuizForge.Providers;

public class GoogleOAuthProvider(
    HttpClient httpClient,
    IConfiguration configuration
) : IOAuthProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _clientId = configuration.GetValue("GoogleAuth:ClientId", string.Empty);
    private readonly string _clientSecret = configuration.GetValue("GoogleAuth:ClientSecret", string.Empty);
    private readonly string _tokenEndpoint =
        configuration.GetValue("GoogleAuth:TokenEndpoint", "https://oauth2.googleapis.com/token");
    private readonly string _userInfoEndpoint =
        configuration.GetValue("GoogleAuth:UserInfoEndpoint", "https://openidconnect.googleapis.com/v1/userinfo");
    private readonly HashSet<string> _allowedRedirectUris =
        configuration.GetSection($"GoogleAuth:AllowedRedirectUris").Get<string[]>() is { Length: > 0 } uris
            ? [.. uris]
            : [];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OAuthTokenDto> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default
    )
    {
        ValidateGoogleConfig(redirectUri);

        var formData = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
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
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _userInfoEndpoint);
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
        if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
            throw new InternalServerException("Thiếu cấu hình GoogleAuth ClientId/ClientSecret");

        if (!_allowedRedirectUris.Contains(redirectUri))
            throw new ValidationException("Uri chuyển hướng không hợp lệ");
    }
}
