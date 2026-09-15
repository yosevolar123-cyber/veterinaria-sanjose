using VetSanJose.Shared.Productos;

namespace VetSanJose.Application.Productos;

public interface IProductosService
{
    Task<List<ProductoTiendaDto>> GetTiendaAsync(long? categoriaId, CancellationToken cancellationToken);
    Task<List<ProductoDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<ProductoDto>> GetStockBajoAsync(int umbral, CancellationToken cancellationToken);
    Task<ProductoDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<ProductoDto> CrearAsync(CrearProductoRequest request, CancellationToken cancellationToken);
    Task<ProductoDto> ActualizarAsync(long id, ActualizarProductoRequest request, CancellationToken cancellationToken);
    Task<ProductoDto> AjustarStockAsync(long id, AjustarStockRequest request, CancellationToken cancellationToken);
}
