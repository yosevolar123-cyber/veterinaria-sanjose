namespace VetSanJose.Domain.Entities;

public class Venta
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public Usuario Cliente { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
