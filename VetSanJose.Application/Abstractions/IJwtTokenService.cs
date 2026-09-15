using VetSanJose.Domain.Entities;

namespace VetSanJose.Application.Abstractions;

public interface IJwtTokenService
{
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }

    string GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}
