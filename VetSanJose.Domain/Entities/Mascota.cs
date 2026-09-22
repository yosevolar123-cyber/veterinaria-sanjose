namespace VetSanJose.Domain.Entities;

public class Mascota
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public decimal? Peso { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Usuario Cliente { get; set; } = null!;
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public ICollection<HistorialMedico> HistorialesMedicos { get; set; } = new List<HistorialMedico>();
}
