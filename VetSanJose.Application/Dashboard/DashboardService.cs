using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Dashboard;

namespace VetSanJose.Application.Dashboard;

public class DashboardService(IAppDbContext db, ICurrentUser currentUser) : IDashboardService
{
    public async Task<DoctorDashboardDto> GetDoctorAsync(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicio = hoy.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);

        var citasHoy = await db.Citas.CountAsync(
            c => c.DoctorId == currentUser.Id && c.FechaHora >= inicio && c.FechaHora < fin, cancellationToken);

        var proximaCita = await db.Citas
            .Where(c => c.DoctorId == currentUser.Id && c.FechaHora >= DateTimeOffset.UtcNow
                && c.Estado != EstadosCita.Cancelada)
            .OrderBy(c => c.FechaHora)
            .Select(c => (DateTimeOffset?)c.FechaHora)
            .FirstOrDefaultAsync(cancellationToken);

        var stockBajo = await db.Productos.CountAsync(
            p => p.Activo && p.Stock <= InventarioConfig.UmbralStockBajoPorDefecto, cancellationToken);

        return new DoctorDashboardDto(citasHoy, proximaCita, stockBajo);
    }

    public async Task<SecretariaDashboardDto> GetSecretariaAsync(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicio = hoy.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);

        var citasHoy = await db.Citas.CountAsync(c => c.FechaHora >= inicio && c.FechaHora < fin, cancellationToken);
        var citasPendientes = await db.Citas.CountAsync(c => c.Estado == EstadosCita.Pendiente, cancellationToken);

        return new SecretariaDashboardDto(citasHoy, citasPendientes);
    }

    public async Task<AdminDashboardDto> GetAdminAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken)
    {
        var inicio = (desde ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30))
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = (hasta ?? DateOnly.FromDateTime(DateTime.UtcNow))
            .ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var ventasPeriodo = await db.Ventas
            .Where(v => v.Fecha >= inicio && v.Fecha <= fin && v.Estado == EstadosVenta.Completada)
            .ToListAsync(cancellationToken);

        var citasPorDoctor = await db.Citas
            .Where(c => c.FechaHora >= inicio && c.FechaHora <= fin)
            .GroupBy(c => new { c.DoctorId, c.Doctor.Nombre, c.Doctor.Apellido })
            .Select(g => new CitasPorDoctorItem(g.Key.DoctorId, g.Key.Nombre + " " + g.Key.Apellido, g.Count()))
            .ToListAsync(cancellationToken);

        var productosStockBajo = await db.Productos
            .Where(p => p.Activo && p.Stock <= InventarioConfig.UmbralStockBajoPorDefecto)
            .OrderBy(p => p.Stock)
            .Select(p => new ProductoStockBajoItem(p.Id, p.Nombre, p.Stock))
            .ToListAsync(cancellationToken);

        return new AdminDashboardDto(
            ventasPeriodo.Sum(v => v.Total),
            ventasPeriodo.Count,
            citasPorDoctor,
            productosStockBajo);
    }
}
