using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuizForge.Exceptions;

namespace QuizForge.Providers;

public class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateAccessToken(long userID)
    {
        var signingKey = _configuration.GetValue("Jwt:SigningKey", "84bfad12-f6d2-4eaf-a00d-fc28fb7759cf");
        var issuer = _configuration.GetValue("Jwt:Issuer", "QuizForge");
        var audience = _configuration.GetValue("Jwt:Audience", "QuizForgeClient");
        var expiresMinutes = _configuration.GetValue("Jwt:AccessTokenExpiresMinutes", 30);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userID.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
