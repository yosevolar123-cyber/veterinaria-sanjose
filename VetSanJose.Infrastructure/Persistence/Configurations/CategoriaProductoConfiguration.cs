using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class CategoriaProductoConfiguration : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> builder)
    {
        builder.ToTable("categorias_producto");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(c => c.Descripcion).HasColumnName("descripcion");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
