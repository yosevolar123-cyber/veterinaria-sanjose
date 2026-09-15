using VetSanJose.Shared.Proveedores;

namespace VetSanJose.Application.Proveedores;

public interface IProveedoresService
{
    Task<List<ProveedorDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProveedorDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<ProveedorDto> CrearAsync(CrearProveedorRequest request, CancellationToken cancellationToken);
    Task<ProveedorDto> ActualizarAsync(long id, ActualizarProveedorRequest request, CancellationToken cancellationToken);
}
