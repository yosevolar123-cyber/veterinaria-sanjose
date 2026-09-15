namespace VetSanJose.Shared.HistorialesMedicos;

public record UsoInsumoDto(long Id, long ProductoId, string ProductoNombre, int Cantidad);

public record HistorialMedicoDto(
    long Id,
    long? CitaId,
    long MascotaId,
    string MascotaNombre,
    long DoctorId,
    string DoctorNombre,
    string? Diagnostico,
    string? Tratamiento,
    string? Observaciones,
    DateTimeOffset Fecha,
    List<UsoInsumoDto> Insumos);

public record InsumoUsadoRequest(long ProductoId, int Cantidad);

public record CrearHistorialRequest(
    long? CitaId,
    long MascotaId,
    string? Diagnostico,
    string? Tratamiento,
    string? Observaciones,
    List<InsumoUsadoRequest> Insumos);
