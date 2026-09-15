using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(p => p.ContactoNombre).HasColumnName("contacto_nombre");
        builder.Property(p => p.Telefono).HasColumnName("telefono");
        builder.Property(p => p.Email).HasColumnName("email");
        builder.Property(p => p.Direccion).HasColumnName("direccion");
        builder.Property(p => p.Activo).HasColumnName("activo");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
    }
}
