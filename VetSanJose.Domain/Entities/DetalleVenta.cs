namespace VetSanJose.Domain.Entities;

public class DetalleVenta
{
    public long Id { get; set; }
    public long VentaId { get; set; }
    public long ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
