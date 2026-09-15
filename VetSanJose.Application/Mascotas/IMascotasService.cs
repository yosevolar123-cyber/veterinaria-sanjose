using VetSanJose.Shared.Mascotas;

namespace VetSanJose.Application.Mascotas;

public interface IMascotasService
{
    Task<List<MascotaDto>> GetMisMascotasAsync(CancellationToken cancellationToken);
    Task<List<MascotaDto>> GetPorClienteAsync(long clienteId, CancellationToken cancellationToken);
    Task<MascotaDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<MascotaDto> CrearAsync(CrearMascotaRequest request, CancellationToken cancellationToken);
    Task<MascotaDto> ActualizarAsync(long id, ActualizarMascotaRequest request, CancellationToken cancellationToken);
}
