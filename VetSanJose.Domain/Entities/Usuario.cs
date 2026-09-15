namespace VetSanJose.Domain.Entities;

public class Usuario
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Especialidad { get; set; }
    public string? Matricula { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
