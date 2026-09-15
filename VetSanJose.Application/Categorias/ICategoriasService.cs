using VetSanJose.Shared.Categorias;

namespace VetSanJose.Application.Categorias;

public interface ICategoriasService
{
    Task<List<CategoriaProductoDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoriaProductoDto> CrearAsync(CrearCategoriaRequest request, CancellationToken cancellationToken);
    Task<CategoriaProductoDto> ActualizarAsync(long id, ActualizarCategoriaRequest request, CancellationToken cancellationToken);
}
