using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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

        // Agregación en la base: se agrupa detalle_ventas por producto y sólo vuelven las filas resumidas.
        // La proyección intermedia es anónima a propósito: EF no puede ordenar por una propiedad de un
        // record proyectado desde un GroupBy (no la reconoce como el agregado) y tira el query al cliente.
        var vendidosAgrupados = await db.DetallesVenta
            .Where(d => d.Venta.Estado == EstadosVenta.Completada && d.Venta.Fecha >= inicio && d.Venta.Fecha < fin)
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

        var productosVendibles = db.Productos.Where(p => p.Activo && p.Tipo == TiposProducto.VentaPublico);

        var idsVendidos = vendidos.Select(p => p.ProductoId).ToList();

        var sinVentasAgrupados = await productosVendibles
            .Where(p => !idsVendidos.Contains(p.Id))
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Id, p.Nombre })
            .ToListAsync(cancellationToken);

        var sinVentas = sinVentasAgrupados
            .Select(p => new ProductoVendidoDto(p.Id, p.Nombre, 0, 0m))
            .ToList();

        // "Menos vendido" es un producto sin ventas si lo hay; si todos vendieron, el de menor cantidad.
        var menosVendido = sinVentas.FirstOrDefault() ?? vendidos.LastOrDefault();

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

        return new ReporteNegocioMesDto(
            anioResuelto,
            mesResuelto,
            ventasMonto,
            ventasCantidad,
            vendidos.FirstOrDefault(),
            menosVendido,
            sinVentas.Count,
            vendidos.Take(5).ToList(),
            inventario?.StockTotal ?? 0,
            inventario?.StockBajo ?? 0,
            InventarioConfig.UmbralStockBajoPorDefecto,
            inventario?.Valor ?? 0m);
    }

    public async Task<byte[]> GetReporteFinancieroPdfAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken)
    {
        var reporte = await GetReporteFinancieroAsync(desde, hasta, cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            container.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text("Veterinaria San José — Reporte financiero e inventario").FontSize(16).Bold();
                    columna.Item().Text($"Período: {reporte.Desde:dd/MM/yyyy} — {reporte.Hasta:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                });

                pagina.Content().PaddingTop(15).Column(columna =>
                {
                    columna.Item().Row(fila =>
                    {
                        fila.RelativeItem().Text($"Ingresos totales: Bs {reporte.TotalIngresos:N2}").Bold();
                        fila.RelativeItem().Text($"Costos de insumos: Bs {reporte.TotalCostos:N2}").Bold();
                    });
                    columna.Item().PaddingBottom(10).Row(fila =>
                    {
                        fila.RelativeItem().Text($"Clientes atendidos: {reporte.TotalClientesAtendidos}").Bold();
                        fila.RelativeItem().Text($"Consultas registradas: {reporte.TotalConsultas}").Bold();
                    });

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                        });

                        tabla.Header(encabezado =>
                        {
                            foreach (var titulo in new[] { "Fecha", "Ingresos (Bs)", "Costos (Bs)", "Clientes", "Consultas" })
                            {
                                encabezado.Cell().Background(Colors.Blue.Lighten4).Padding(4).Text(titulo).Bold();
                            }
                        });

                        foreach (var item in reporte.Items)
                        {
                            tabla.Cell().Padding(4).Text(item.Fecha.ToString("dd/MM/yyyy"));
                            tabla.Cell().Padding(4).Text(item.Ingresos.ToString("N2"));
                            tabla.Cell().Padding(4).Text(item.Costos.ToString("N2"));
                            tabla.Cell().Padding(4).Text(item.ClientesAtendidos.ToString());
                            tabla.Cell().Padding(4).Text(item.Consultas.ToString());
                        }
                    });
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado el ").FontColor(Colors.Grey.Darken1);
                    texto.Span(DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm")).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return await Task.Run(() => documento.GeneratePdf(), cancellationToken);
    }

    private static (DateOnly InicioDia, DateOnly FinDia, DateTimeOffset Inicio, DateTimeOffset Fin) ResolverRango(DateOnly? desde, DateOnly? hasta)
    {
        var inicioDia = desde ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);
        var finDia = hasta ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var inicio = inicioDia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = finDia.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return (inicioDia, finDia, inicio, fin);
    }
}
