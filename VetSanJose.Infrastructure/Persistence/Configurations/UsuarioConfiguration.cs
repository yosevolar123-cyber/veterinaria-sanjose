using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(u => u.Apellido).HasColumnName("apellido").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasColumnType("citext").IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.Rol).HasColumnName("rol").IsRequired();
        builder.Property(u => u.Telefono).HasColumnName("telefono");
        builder.Property(u => u.Especialidad).HasColumnName("especialidad");
        builder.Property(u => u.Matricula).HasColumnName("matricula");
        builder.Property(u => u.Activo).HasColumnName("activo");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
