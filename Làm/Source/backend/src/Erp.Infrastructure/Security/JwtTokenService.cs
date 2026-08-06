using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Erp.Infrastructure.Security;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(Guid userId, Guid tenantId, string username, IEnumerable<string> roles);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(
        Guid userId, Guid tenantId, string username, IEnumerable<string> roles)
    {
        var key = _config["Jwt:SecretKey"] ?? "PumsErp_DevSecretKey_ChangeMe_AtLeast32Chars!";
        var issuer = _config["Jwt:Issuer"] ?? "pums-erp-api";
        var audience = _config["Jwt:Audience"] ?? "pums-erp-app";
        var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 120;
        var expires = DateTimeOffset.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
