using VetSanJose.Shared.HistorialesMedicos;

namespace VetSanJose.Application.HistorialesMedicos;

public interface IHistorialMedicoService
{
    Task<List<HistorialMedicoDto>> GetPorMascotaAsync(long mascotaId, CancellationToken cancellationToken);
    Task<HistorialMedicoDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<HistorialMedicoDto> CrearAsync(CrearHistorialRequest request, CancellationToken cancellationToken);
}
