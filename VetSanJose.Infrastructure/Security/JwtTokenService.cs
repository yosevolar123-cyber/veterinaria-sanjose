using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(_settings.AccessTokenMinutes);
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_settings.RefreshTokenDays);

    public string GenerateAccessToken(Usuario usuario)
    {
        var claves = new SymmetricSecurityKey(Convert.FromBase64String(_settings.SigningKey));
        var credenciales = new SigningCredentials(claves, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol),
            new("nombre", usuario.Nombre),
            new("apellido", usuario.Apellido),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(AccessTokenLifetime),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
