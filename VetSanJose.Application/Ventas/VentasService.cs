using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Ventas;

namespace VetSanJose.Application.Ventas;

public class VentasService(IAppDbContext db, ICurrentUser currentUser) : IVentasService
{
    public async Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken cancellationToken)
    {
        if (!currentUser.EsCliente)
        {
            throw new ForbiddenAppException("Solo un cliente puede generar una compra.");
        }

        var productoIds = request.Items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var venta = new Venta
        {
            ClienteId = currentUser.Id,
            Fecha = DateTimeOffset.UtcNow,
            Estado = EstadosVenta.Completada,
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto) || !producto.Activo)
            {
                throw new NotFoundException($"El producto {item.ProductoId} no existe o no está disponible.");
            }

            total += producto.Precio * item.Cantidad;

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
            });
        }

        venta.Total = total;

        db.Ventas.Add(venta);
        await db.SaveChangesTraduciendoErroresAsync(cancellationToken);

        return Mapear(await BuscarAsync(venta.Id, cancellationToken));
    }

    public async Task<List<VentaDto>> GetMisVentasAsync(CancellationToken cancellationToken)
    {
        var ventas = await Consulta()
            .Where(v => v.ClienteId == currentUser.Id)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);

        return ventas.Select(Mapear).ToList();
    }

    public async Task<List<VentaDto>> GetPorClienteAsync(long clienteId, CancellationToken cancellationToken)
    {
        if (currentUser.EsCliente && currentUser.Id != clienteId)
        {
            throw new ForbiddenAppException("No puede consultar compras de otro cliente.");
        }

        if (!currentUser.EsCliente && !currentUser.EsAdministrador)
        {
            throw new ForbiddenAppException("No tiene permisos para consultar compras.");
        }

        var ventas = await Consulta()
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);

        return ventas.Select(Mapear).ToList();
    }

    public async Task<VentaDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var venta = await BuscarAsync(id, cancellationToken);

        if (currentUser.EsCliente && venta.ClienteId != currentUser.Id)
        {
            throw new ForbiddenAppException("No puede acceder a compras de otro cliente.");
        }

        if (!currentUser.EsCliente && !currentUser.EsAdministrador)
        {
            throw new ForbiddenAppException("No tiene permisos para consultar esta compra.");
        }

        return Mapear(venta);
    }

    private async Task<Venta> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await Consulta().FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException("Venta no encontrada.");
    }

    private IQueryable<Venta> Consulta() => db.Ventas
        .Include(v => v.Detalles).ThenInclude(d => d.Producto);

    private static VentaDto Mapear(Venta v) => new(
        v.Id, v.ClienteId, v.Fecha, v.Total, v.Estado,
        v.Detalles.Select(d => new DetalleVentaDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal)).ToList());
}
