using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Shared.Reportes;

namespace VetSanJose.Application.Reportes;

public class ReportesService(IAppDbContext db) : IReportesService
{
    public async Task<ReporteFinancieroDto> GetReporteFinancieroAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken)
    {
        var (inicioDia, finDia, inicio, fin) = ResolverRango(desde, hasta);

        var ventas = await db.Ventas
            .Where(v => v.Estado == EstadosVenta.Completada && v.Fecha >= inicio && v.Fecha <= fin)
            .Select(v => new { v.Fecha, v.Total })
            .ToListAsync(cancellationToken);

        var usosInsumo = await db.UsosInsumo
            .Where(u => u.Historial.Fecha >= inicio && u.Historial.Fecha <= fin)
            .Select(u => new { u.Historial.Fecha, Costo = u.Cantidad * u.Producto.Precio })
            .ToListAsync(cancellationToken);

        var historiales = await db.HistorialesMedicos
            .Where(h => h.Fecha >= inicio && h.Fecha <= fin)
            .Select(h => new { h.Fecha, h.Mascota.ClienteId })
            .ToListAsync(cancellationToken);

        var ingresosPorDia = ventas
            .GroupBy(v => DateOnly.FromDateTime(v.Fecha.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

        var costosPorDia = usosInsumo
            .GroupBy(u => DateOnly.FromDateTime(u.Fecha.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Sum(u => u.Costo));

        var historialesPorDia = historiales
            .GroupBy(h => DateOnly.FromDateTime(h.Fecha.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<ReporteFinancieroItemDto>();
        for (var dia = inicioDia; dia <= finDia; dia = dia.AddDays(1))
        {
            var historialesDia = historialesPorDia.GetValueOrDefault(dia, []);
            items.Add(new ReporteFinancieroItemDto(
                dia,
                ingresosPorDia.GetValueOrDefault(dia),
                costosPorDia.GetValueOrDefault(dia),
                historialesDia.Select(h => h.ClienteId).Distinct().Count(),
                historialesDia.Count));
        }

        return new ReporteFinancieroDto(
            inicioDia,
            finDia,
            items,
            ventas.Sum(v => v.Total),
            usosInsumo.Sum(u => u.Costo),
            historiales.Select(h => h.ClienteId).Distinct().Count(),
            historiales.Count);
    }

    public async Task<ReporteNegocioMesDto> GetReporteNegocioMesAsync(int? anio, int? mes, CancellationToken cancellationToken)
    {
        var hoy = DateTime.UtcNow;
        var anioResuelto = anio ?? hoy.Year;
        var mesResuelto = mes ?? hoy.Month;

        if (mesResuelto is < 1 or > 12)
        {
            throw new AppException("El mes debe estar entre 1 y 12.");
        }

        var inicio = new DateTimeOffset(new DateTime(anioResuelto, mesResuelto, 1, 0, 0, 0, DateTimeKind.Utc));
        var fin = inicio.AddMonths(1);

        var ventasDelMes = db.Ventas
            .Where(v => v.Estado == EstadosVenta.Completada && v.Fecha >= inicio && v.Fecha < fin);

        var ventasCantidad = await ventasDelMes.CountAsync(cancellationToken);
        var ventasMonto = await ventasDelMes.SumAsync(v => (decimal?)v.Total, cancellationToken) ?? 0m;

        var ranking = await GetRankingProductosAsync(inicio, fin, cancellationToken);
        var inventario = await GetResumenInventarioAsync(cancellationToken);

        return new ReporteNegocioMesDto(
            anioResuelto,
            mesResuelto,
            ventasMonto,
            ventasCantidad,
            ranking.MasVendido,
            ranking.MenosVendido,
            ranking.SinVentas,
            ranking.Top,
            inventario.StockTotal,
            inventario.StockBajo,
            InventarioConfig.UmbralStockBajoPorDefecto,
            inventario.Valor);
    }

    // Ranking de productos vendidos en [inicio, finExclusivo). Lo comparten el reporte mensual y el PDF.
    private async Task<RankingProductos> GetRankingProductosAsync(
        DateTimeOffset inicio, DateTimeOffset finExclusivo, CancellationToken cancellationToken)
    {
        // Agregación en la base: se agrupa detalle_ventas por producto y sólo vuelven las filas resumidas.
        // La proyección intermedia es anónima a propósito: EF no puede ordenar por una propiedad de un
        // record proyectado desde un GroupBy (no la reconoce como el agregado) y tira el query al cliente.
        var vendidosAgrupados = await db.DetallesVenta
            .Where(d => d.Venta.Estado == EstadosVenta.Completada && d.Venta.Fecha >= inicio && d.Venta.Fecha < finExclusivo)
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
            .Select(g => new
            {
                g.Key.ProductoId,
                g.Key.Nombre,
                Cantidad = g.Sum(d => d.Cantidad),
                Total = g.Sum(d => d.Subtotal),
            })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync(cancellationToken);

        var vendidos = vendidosAgrupados
            .Select(x => new ProductoVendidoDto(x.ProductoId, x.Nombre, x.Cantidad, x.Total))
            .ToList();

        var idsVendidos = vendidos.Select(p => p.ProductoId).ToList();

        var sinVentasAgrupados = await db.Productos
            .Where(p => p.Activo && p.Tipo == TiposProducto.VentaPublico && !idsVendidos.Contains(p.Id))
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Id, p.Nombre })
            .ToListAsync(cancellationToken);

        var sinVentas = sinVentasAgrupados
            .Select(p => new ProductoVendidoDto(p.Id, p.Nombre, 0, 0m))
            .ToList();

        // "Menos vendido" es un producto sin ventas si lo hay; si todos vendieron, el de menor cantidad.
        var menosVendido = sinVentas.FirstOrDefault() ?? vendidos.LastOrDefault();

        return new RankingProductos(vendidos.FirstOrDefault(), menosVendido, sinVentas.Count, vendidos.Take(5).ToList());
    }

    private async Task<ResumenInventario> GetResumenInventarioAsync(CancellationToken cancellationToken)
    {
        var inventario = await db.Productos
            .Where(p => p.Activo)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                StockTotal = g.Sum(p => p.Stock),
                Valor = g.Sum(p => p.Precio * p.Stock),
                StockBajo = g.Count(p => p.Stock <= InventarioConfig.UmbralStockBajoPorDefecto),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ResumenInventario(inventario?.StockTotal ?? 0, inventario?.StockBajo ?? 0, inventario?.Valor ?? 0m);
    }

    public async Task<byte[]> GetReporteFinancieroPdfAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken)
    {
        var reporte = await GetReporteFinancieroAsync(desde, hasta, cancellationToken);

        var inicio = reporte.Desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var finExclusivo = reporte.Hasta.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var ranking = await GetRankingProductosAsync(inicio, finExclusivo, cancellationToken);
        var inventario = await GetResumenInventarioAsync(cancellationToken);

        var datos = new ReporteFinancieroPdf.Datos(
            reporte,
            ranking.MasVendido,
            ranking.MenosVendido,
            inventario.Valor,
            inventario.StockBajo,
            InventarioConfig.UmbralStockBajoPorDefecto);

        return await Task.Run(() => ReporteFinancieroPdf.Generar(datos), cancellationToken);
    }

    private static (DateOnly InicioDia, DateOnly FinDia, DateTimeOffset Inicio, DateTimeOffset Fin) ResolverRango(DateOnly? desde, DateOnly? hasta)
    {
        var inicioDia = desde ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);
        var finDia = hasta ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var inicio = inicioDia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = finDia.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return (inicioDia, finDia, inicio, fin);
    }

    private record RankingProductos(
        ProductoVendidoDto? MasVendido, ProductoVendidoDto? MenosVendido, int SinVentas, List<ProductoVendidoDto> Top);

    private record ResumenInventario(int StockTotal, int StockBajo, decimal Valor);
}
