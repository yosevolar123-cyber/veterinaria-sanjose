namespace VetSanJose.Domain.Entities;

public class UsoInsumo
{
    public long Id { get; set; }
    public long HistorialId { get; set; }
    public long ProductoId { get; set; }
    public int Cantidad { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public HistorialMedico Historial { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
