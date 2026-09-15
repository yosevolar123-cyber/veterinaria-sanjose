namespace VetSanJose.Domain.Entities;

public class CategoriaProducto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
