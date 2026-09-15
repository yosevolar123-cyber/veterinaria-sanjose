namespace VetSanJose.Domain.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public Usuario Usuario { get; set; } = null!;
}
