using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Productos;

namespace VetSanJose.Application.Productos;

public class ProductosService(IAppDbContext db) : IProductosService
{
    public async Task<List<ProductoTiendaDto>> GetTiendaAsync(long? categoriaId, CancellationToken cancellationToken)
    {
        var query = db.Productos.Where(p => p.Activo && p.Tipo == TiposProducto.VentaPublico);

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoTiendaDto(
                p.Id, p.Nombre, p.Descripcion, p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : null,
                p.Precio, p.ImagenUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProductoDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Productos
            .OrderBy(p => p.Nombre)
            .Select(MapExpressionDef)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProductoDto>> GetStockBajoAsync(int umbral, CancellationToken cancellationToken)
    {
        return await db.Productos
            .Where(p => p.Activo && p.Stock <= umbral)
            .OrderBy(p => p.Stock)
            .Select(MapExpressionDef)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductoDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var producto = await BuscarConNavegacionAsync(id, cancellationToken);
        return Mapear(producto);
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        await ValidarReferenciasAsync(request.CategoriaId, request.ProveedorId, cancellationToken);

        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            Precio = request.Precio,
            Stock = request.Stock,
            ImagenUrl = request.ImagenUrl,
            ProveedorId = request.ProveedorId,
            Tipo = request.Tipo,
            Activo = true,
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(await BuscarConNavegacionAsync(producto.Id, cancellationToken));
    }

    public async Task<ProductoDto> ActualizarAsync(long id, ActualizarProductoRequest request, CancellationToken cancellationToken)
    {
        await ValidarReferenciasAsync(request.CategoriaId, request.ProveedorId, cancellationToken);

        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.CategoriaId = request.CategoriaId;
        producto.Precio = request.Precio;
        producto.ImagenUrl = request.ImagenUrl;
        producto.ProveedorId = request.ProveedorId;
        producto.Tipo = request.Tipo;
        producto.Activo = request.Activo;

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(await BuscarConNavegacionAsync(id, cancellationToken));
    }

    public async Task<ProductoDto> AjustarStockAsync(long id, AjustarStockRequest request, CancellationToken cancellationToken)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");

        var nuevoStock = producto.Stock + request.Cantidad;
        if (nuevoStock < 0)
        {
            throw new AppException($"El ajuste dejaría el stock en negativo (actual: {producto.Stock}).");
        }

        producto.Stock = nuevoStock;
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(await BuscarConNavegacionAsync(id, cancellationToken));
    }

    private async Task ValidarReferenciasAsync(long? categoriaId, long? proveedorId, CancellationToken cancellationToken)
    {
        if (categoriaId.HasValue && !await db.CategoriasProducto.AnyAsync(c => c.Id == categoriaId.Value, cancellationToken))
        {
            throw new NotFoundException("La categoría especificada no existe.");
        }

        if (proveedorId.HasValue && !await db.Proveedores.AnyAsync(p => p.Id == proveedorId.Value, cancellationToken))
        {
            throw new NotFoundException("El proveedor especificado no existe.");
        }
    }

    private async Task<Producto> BuscarConNavegacionAsync(long id, CancellationToken cancellationToken)
    {
        return await db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Producto no encontrado.");
    }

    private static ProductoDto Mapear(Producto p) => new(
        p.Id, p.Nombre, p.Descripcion, p.CategoriaId, p.Categoria?.Nombre,
        p.Precio, p.Stock, p.ImagenUrl, p.ProveedorId, p.Proveedor?.Nombre, p.Activo, p.Tipo);

    private static readonly System.Linq.Expressions.Expression<Func<Producto, ProductoDto>> MapExpressionDef = p =>
        new ProductoDto(
            p.Id, p.Nombre, p.Descripcion, p.CategoriaId,
            p.Categoria != null ? p.Categoria.Nombre : null,
            p.Precio, p.Stock, p.ImagenUrl, p.ProveedorId,
            p.Proveedor != null ? p.Proveedor.Nombre : null, p.Activo, p.Tipo);
}
