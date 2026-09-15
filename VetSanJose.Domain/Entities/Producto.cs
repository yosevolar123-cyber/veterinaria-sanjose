namespace VetSanJose.Domain.Entities;

public class Producto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public long? CategoriaId { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public long? ProveedorId { get; set; }
    public string Tipo { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public CategoriaProducto? Categoria { get; set; }
    public Proveedor? Proveedor { get; set; }
    public ICollection<UsoInsumo> UsosInsumo { get; set; } = new List<UsoInsumo>();
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}
