using VetSanJose.Shared.Citas;

namespace VetSanJose.Application.Citas;

public interface ICitasService
{
    Task<List<CitaDto>> GetMisCitasAsync(CancellationToken cancellationToken);
    Task<List<CitaDto>> GetAgendaAsync(long? doctorId, DateOnly? fecha, CancellationToken cancellationToken);
    Task<CitaDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<CitaDto> CrearAsync(CrearCitaRequest request, CancellationToken cancellationToken);
    Task<CitaDto> ActualizarAsync(long id, ActualizarCitaRequest request, CancellationToken cancellationToken);
}
