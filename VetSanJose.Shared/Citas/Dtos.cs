namespace VetSanJose.Shared.Citas;

public record CitaDto(
    long Id,
    long MascotaId,
    string MascotaNombre,
    long ClienteId,
    string ClienteNombre,
    long DoctorId,
    string DoctorNombre,
    DateTimeOffset FechaHora,
    string? Motivo,
    string Estado);

public record CrearCitaRequest(
    long MascotaId,
    long? DoctorId,
    DateTimeOffset FechaHora,
    string? Motivo);

public record ActualizarCitaRequest(
    DateTimeOffset FechaHora,
    string? Motivo,
    string Estado);
