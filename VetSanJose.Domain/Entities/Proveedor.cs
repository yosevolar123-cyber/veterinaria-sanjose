namespace VetSanJose.Domain.Entities;

public class Proveedor
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? ContactoNombre { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
