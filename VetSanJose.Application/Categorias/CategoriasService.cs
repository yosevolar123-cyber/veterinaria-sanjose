using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Categorias;

namespace VetSanJose.Application.Categorias;

public class CategoriasService(IAppDbContext db) : ICategoriasService
{
    public async Task<List<CategoriaProductoDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.CategoriasProducto
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaProductoDto(c.Id, c.Nombre, c.Descripcion))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaProductoDto> CrearAsync(CrearCategoriaRequest request, CancellationToken cancellationToken)
    {
        var existe = await db.CategoriasProducto.AnyAsync(c => c.Nombre == request.Nombre, cancellationToken);
        if (existe)
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        var categoria = new CategoriaProducto { Nombre = request.Nombre, Descripcion = request.Descripcion };
        db.CategoriasProducto.Add(categoria);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoriaProductoDto(categoria.Id, categoria.Nombre, categoria.Descripcion);
    }

    public async Task<CategoriaProductoDto> ActualizarAsync(long id, ActualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var categoria = await db.CategoriasProducto.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoría no encontrada.");

        categoria.Nombre = request.Nombre;
        categoria.Descripcion = request.Descripcion;
        await db.SaveChangesAsync(cancellationToken);

        return new CategoriaProductoDto(categoria.Id, categoria.Nombre, categoria.Descripcion);
    }
}
