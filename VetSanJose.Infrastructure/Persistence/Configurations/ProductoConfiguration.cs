using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(p => p.Descripcion).HasColumnName("descripcion");
        builder.Property(p => p.CategoriaId).HasColumnName("categoria_id");
        builder.Property(p => p.Precio).HasColumnName("precio").HasColumnType("numeric(10,2)");
        builder.Property(p => p.Stock).HasColumnName("stock");
        builder.Property(p => p.ImagenUrl).HasColumnName("imagen_url");
        builder.Property(p => p.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(p => p.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(p => p.Activo).HasColumnName("activo");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Proveedor)
            .WithMany(pr => pr.Productos)
            .HasForeignKey(p => p.ProveedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
