using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.HistorialesMedicos;

namespace VetSanJose.Application.HistorialesMedicos;

public class HistorialMedicoService(IAppDbContext db, ICurrentUser currentUser) : IHistorialMedicoService
{
    public async Task<List<HistorialMedicoDto>> GetPorMascotaAsync(long mascotaId, CancellationToken cancellationToken)
    {
        var mascota = await db.Mascotas.FirstOrDefaultAsync(m => m.Id == mascotaId, cancellationToken)
            ?? throw new NotFoundException("Mascota no encontrada.");

        VerificarAccesoMascota(mascota);

        var historiales = await Consulta()
            .Where(h => h.MascotaId == mascotaId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync(cancellationToken);

        return historiales.Select(Mapear).ToList();
    }

    public async Task<HistorialMedicoDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var historial = await BuscarAsync(id, cancellationToken);
        VerificarAccesoMascota(historial.Mascota);
        return Mapear(historial);
    }

    public async Task<HistorialMedicoDto> CrearAsync(CrearHistorialRequest request, CancellationToken cancellationToken)
    {
        if (!currentUser.EsDoctor)
        {
            throw new ForbiddenAppException("Solo un doctor puede registrar historial médico.");
        }

        var mascota = await db.Mascotas.FirstOrDefaultAsync(m => m.Id == request.MascotaId, cancellationToken)
            ?? throw new NotFoundException("Mascota no encontrada.");

        if (request.CitaId.HasValue)
        {
            var citaValida = await db.Citas.AnyAsync(
                c => c.Id == request.CitaId.Value && c.MascotaId == request.MascotaId && c.DoctorId == currentUser.Id,
                cancellationToken);

            if (!citaValida)
            {
                throw new NotFoundException("La cita especificada no corresponde a este doctor y mascota.");
            }
        }

        var historial = new Domain.Entities.HistorialMedico
        {
            CitaId = request.CitaId,
            MascotaId = mascota.Id,
            DoctorId = currentUser.Id,
            Diagnostico = request.Diagnostico,
            Tratamiento = request.Tratamiento,
            Observaciones = request.Observaciones,
            Fecha = DateTimeOffset.UtcNow,
        };

        foreach (var insumo in request.Insumos)
        {
            var productoExiste = await db.Productos.AnyAsync(p => p.Id == insumo.ProductoId, cancellationToken);
            if (!productoExiste)
            {
                throw new NotFoundException($"El producto {insumo.ProductoId} no existe.");
            }

            historial.UsosInsumo.Add(new UsoInsumo
            {
                ProductoId = insumo.ProductoId,
                Cantidad = insumo.Cantidad,
            });
        }

        db.HistorialesMedicos.Add(historial);
        await db.SaveChangesTraduciendoErroresAsync(cancellationToken);

        return Mapear(await BuscarAsync(historial.Id, cancellationToken));
    }

    private void VerificarAccesoMascota(Mascota mascota)
    {
        if (currentUser.EsCliente && mascota.ClienteId != currentUser.Id)
        {
            throw new ForbiddenAppException("No puede acceder al historial médico de otro cliente.");
        }

        if (currentUser.EsSecretaria)
        {
            throw new ForbiddenAppException("No tiene permisos para consultar historiales médicos.");
        }
    }

    private async Task<Domain.Entities.HistorialMedico> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await Consulta().FirstOrDefaultAsync(h => h.Id == id, cancellationToken)
            ?? throw new NotFoundException("Historial médico no encontrado.");
    }

    private IQueryable<Domain.Entities.HistorialMedico> Consulta() => db.HistorialesMedicos
        .Include(h => h.Mascota)
        .Include(h => h.Doctor)
        .Include(h => h.UsosInsumo).ThenInclude(u => u.Producto);

    private static HistorialMedicoDto Mapear(Domain.Entities.HistorialMedico h) => new(
        h.Id, h.CitaId, h.MascotaId, h.Mascota.Nombre, h.DoctorId, $"{h.Doctor.Nombre} {h.Doctor.Apellido}",
        h.Diagnostico, h.Tratamiento, h.Observaciones, h.Fecha,
        h.UsosInsumo.Select(u => new UsoInsumoDto(u.Id, u.ProductoId, u.Producto.Nombre, u.Cantidad)).ToList());
}
