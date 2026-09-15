using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.ClienteId).HasColumnName("cliente_id");
        builder.Property(v => v.Fecha).HasColumnName("fecha");
        builder.Property(v => v.Total).HasColumnName("total").HasColumnType("numeric(10,2)");
        builder.Property(v => v.Estado).HasColumnName("estado");
        builder.Property(v => v.CreatedAt).HasColumnName("created_at");

        builder.HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("detalle_ventas");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.VentaId).HasColumnName("venta_id");
        builder.Property(d => d.ProductoId).HasColumnName("producto_id");
        builder.Property(d => d.Cantidad).HasColumnName("cantidad");
        builder.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(10,2)");
        builder.Property(d => d.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(10,2)")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
