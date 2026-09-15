using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Proveedores;

namespace VetSanJose.Application.Proveedores;

public class ProveedoresService(IAppDbContext db) : IProveedoresService
{
    public async Task<List<ProveedorDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Proveedores
            .OrderBy(p => p.Nombre)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProveedorDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    public async Task<ProveedorDto> CrearAsync(CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        var proveedor = new Proveedor
        {
            Nombre = request.Nombre,
            ContactoNombre = request.ContactoNombre,
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            Activo = true,
        };

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(proveedor);
    }

    public async Task<ProveedorDto> ActualizarAsync(long id, ActualizarProveedorRequest request, CancellationToken cancellationToken)
    {
        var proveedor = await BuscarAsync(id, cancellationToken);

        proveedor.Nombre = request.Nombre;
        proveedor.ContactoNombre = request.ContactoNombre;
        proveedor.Telefono = request.Telefono;
        proveedor.Email = request.Email;
        proveedor.Direccion = request.Direccion;
        proveedor.Activo = request.Activo;

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(proveedor);
    }

    private async Task<Proveedor> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Proveedor no encontrado.");
    }

    private static ProveedorDto Mapear(Proveedor p) =>
        new(p.Id, p.Nombre, p.ContactoNombre, p.Telefono, p.Email, p.Direccion, p.Activo);

    private static readonly System.Linq.Expressions.Expression<Func<Proveedor, ProveedorDto>> MapExpression = p =>
        new ProveedorDto(p.Id, p.Nombre, p.ContactoNombre, p.Telefono, p.Email, p.Direccion, p.Activo);
}
