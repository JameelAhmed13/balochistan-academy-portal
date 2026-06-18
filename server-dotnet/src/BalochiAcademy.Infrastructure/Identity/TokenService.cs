using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BalochiAcademy.Infrastructure.Identity;

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly string _secret   = config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret not configured");
    private readonly string _issuer   = config["Jwt:Issuer"]   ?? "BalochiAcademy";
    private readonly string _audience = config["Jwt:Audience"] ?? "BalochiAcademyUsers";
    private readonly int    _accessMinutes  = int.Parse(config["Jwt:AccessExpiryMinutes"]  ?? "60");
    private readonly int    _refreshMinutes = int.Parse(config["Jwt:RefreshExpiryMinutes"] ?? "43200");

    public string GenerateAccessToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_accessMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier,   user.Id.ToString()),
            new Claim(ClaimTypes.Name,              user.Username),
            new Claim(ClaimTypes.Role,              user.Role?.Name ?? "student"),
            new Claim("grade",                      user.GradeCode ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer, audience: _audience,
            claims: claims, expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(int userId) => new()
    {
        UserId    = userId,
        Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        ExpiresAt = DateTime.UtcNow.AddMinutes(_refreshMinutes),
        CreatedAt = DateTime.UtcNow,
    };

    public int? ValidateAccessToken(string token)
    {
        try
        {
            var handler   = new JwtSecurityTokenHandler();
            var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _issuer,
                ValidateAudience         = true,
                ValidAudience            = _audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero,
            }, out _);

            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
        catch { return null; }
    }
}
