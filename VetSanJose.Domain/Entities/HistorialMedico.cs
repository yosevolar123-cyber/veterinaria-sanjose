namespace VetSanJose.Domain.Entities;

public class HistorialMedico
{
    public long Id { get; set; }
    public long? CitaId { get; set; }
    public long MascotaId { get; set; }
    public long DoctorId { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Cita? Cita { get; set; }
    public Mascota Mascota { get; set; } = null!;
    public Usuario Doctor { get; set; } = null!;
    public ICollection<UsoInsumo> UsosInsumo { get; set; } = new List<UsoInsumo>();
}
