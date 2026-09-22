using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("mascotas");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ClienteId).HasColumnName("cliente_id");
        builder.Property(m => m.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(m => m.Especie).HasColumnName("especie").IsRequired();
        builder.Property(m => m.Raza).HasColumnName("raza");
        builder.Property(m => m.FechaNacimiento).HasColumnName("fecha_nacimiento");
        builder.Property(m => m.Sexo).HasColumnName("sexo");
        builder.Property(m => m.Peso).HasColumnName("peso").HasColumnType("numeric(6,2)");
        builder.Property(m => m.ImagenUrl).HasColumnName("imagen_url");
        builder.Property(m => m.Activo).HasColumnName("activo");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(m => m.Cliente)
            .WithMany(u => u.Mascotas)
            .HasForeignKey(m => m.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
