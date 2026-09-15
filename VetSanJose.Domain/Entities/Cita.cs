namespace VetSanJose.Domain.Entities;

public class Cita
{
    public long Id { get; set; }
    public long MascotaId { get; set; }
    public long DoctorId { get; set; }
    public long CreadoPor { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Mascota Mascota { get; set; } = null!;
    public Usuario Doctor { get; set; } = null!;
    public Usuario CreadoPorUsuario { get; set; } = null!;
    public ICollection<HistorialMedico> HistorialesMedicos { get; set; } = new List<HistorialMedico>();
}
