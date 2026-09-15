using VetSanJose.Shared.Ventas;

namespace VetSanJose.Application.Ventas;

public interface IVentasService
{
    Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken cancellationToken);
    Task<List<VentaDto>> GetMisVentasAsync(CancellationToken cancellationToken);
    Task<List<VentaDto>> GetPorClienteAsync(long clienteId, CancellationToken cancellationToken);
    Task<VentaDto> GetByIdAsync(long id, CancellationToken cancellationToken);
}
