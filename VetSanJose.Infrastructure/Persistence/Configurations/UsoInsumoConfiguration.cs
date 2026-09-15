using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class UsoInsumoConfiguration : IEntityTypeConfiguration<UsoInsumo>
{
    public void Configure(EntityTypeBuilder<UsoInsumo> builder)
    {
        builder.ToTable("uso_insumos");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.HistorialId).HasColumnName("historial_id");
        builder.Property(u => u.ProductoId).HasColumnName("producto_id");
        builder.Property(u => u.Cantidad).HasColumnName("cantidad");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");

        builder.HasOne(u => u.Historial)
            .WithMany(h => h.UsosInsumo)
            .HasForeignKey(u => u.HistorialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Producto)
            .WithMany(p => p.UsosInsumo)
            .HasForeignKey(u => u.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
