using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.UsuarioId).HasColumnName("usuario_id");
        builder.Property(r => r.TokenHash).HasColumnName("token_hash").IsRequired();
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at");
        builder.Property(r => r.RevokedAt).HasColumnName("revoked_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");

        builder.Ignore(r => r.IsActive);

        builder.HasIndex(r => r.TokenHash).IsUnique();

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
