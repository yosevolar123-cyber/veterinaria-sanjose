using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Citas;

namespace VetSanJose.Application.Citas;

public class CitasService(IAppDbContext db, ICurrentUser currentUser) : ICitasService
{
    public async Task<List<CitaDto>> GetMisCitasAsync(CancellationToken cancellationToken)
    {
        return await Consulta()
            .Where(c => c.Mascota.ClienteId == currentUser.Id)
            .OrderByDescending(c => c.FechaHora)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CitaDto>> GetAgendaAsync(long? doctorId, DateOnly? fecha, CancellationToken cancellationToken)
    {
        if (currentUser.EsCliente)
        {
            throw new ForbiddenAppException("No tiene permisos para consultar la agenda.");
        }

        long? doctorFiltro = doctorId;
        if (currentUser.EsDoctor)
        {
            doctorFiltro = currentUser.Id;
        }

        var query = Consulta();

        if (doctorFiltro.HasValue)
        {
            query = query.Where(c => c.DoctorId == doctorFiltro.Value);
        }

        if (fecha.HasValue)
        {
            var inicio = fecha.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var fin = inicio.AddDays(1);
            query = query.Where(c => c.FechaHora >= inicio && c.FechaHora < fin);
        }

        return await query
            .OrderBy(c => c.FechaHora)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<CitaDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var cita = await BuscarAsync(id, cancellationToken);
        VerificarAcceso(cita);
        return Mapear(cita);
    }

    public async Task<CitaDto> CrearAsync(CrearCitaRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.EsCliente)
        {
            throw new ForbiddenAppException("No tiene permisos para agendar citas.");
        }

        var mascota = await db.Mascotas.FirstOrDefaultAsync(m => m.Id == request.MascotaId, cancellationToken)
            ?? throw new NotFoundException("Mascota no encontrada.");

        long doctorId;
        if (currentUser.EsDoctor)
        {
            doctorId = currentUser.Id;
        }
        else
        {
            doctorId = request.DoctorId ?? throw new AppException("Debe especificar el doctor para la cita.");
        }

        var doctor = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == doctorId && u.Rol == Roles.Doctor, cancellationToken)
            ?? throw new NotFoundException("El doctor especificado no existe.");

        var cita = new Cita
        {
            MascotaId = mascota.Id,
            DoctorId = doctor.Id,
            CreadoPor = currentUser.Id,
            FechaHora = request.FechaHora.ToUniversalTime(),
            Motivo = request.Motivo,
            Estado = EstadosCita.Pendiente,
        };

        db.Citas.Add(cita);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(await BuscarAsync(cita.Id, cancellationToken));
    }

    public async Task<CitaDto> ActualizarAsync(long id, ActualizarCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await BuscarAsync(id, cancellationToken);
        VerificarAcceso(cita);

        if (currentUser.EsCliente)
        {
            throw new ForbiddenAppException("No tiene permisos para modificar citas.");
        }

        cita.FechaHora = request.FechaHora.ToUniversalTime();
        cita.Motivo = request.Motivo;
        cita.Estado = request.Estado;

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    private void VerificarAcceso(Cita cita)
    {
        if (currentUser.EsCliente && cita.Mascota.ClienteId != currentUser.Id)
        {
            throw new ForbiddenAppException("No puede acceder a citas de otro cliente.");
        }

        if (currentUser.EsDoctor && cita.DoctorId != currentUser.Id)
        {
            throw new ForbiddenAppException("No puede acceder a citas de otro doctor.");
        }
    }

    private async Task<Cita> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await Consulta().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Cita no encontrada.");
    }

    private IQueryable<Cita> Consulta() => db.Citas
        .Include(c => c.Mascota).ThenInclude(m => m.Cliente)
        .Include(c => c.Doctor);

    private static CitaDto Mapear(Cita c) => new(
        c.Id, c.MascotaId, c.Mascota.Nombre, c.Mascota.ClienteId,
        $"{c.Mascota.Cliente.Nombre} {c.Mascota.Cliente.Apellido}", c.DoctorId, $"{c.Doctor.Nombre} {c.Doctor.Apellido}",
        c.FechaHora, c.Motivo, c.Estado);

    private static readonly System.Linq.Expressions.Expression<Func<Cita, CitaDto>> MapExpression = c =>
        new CitaDto(
            c.Id, c.MascotaId, c.Mascota.Nombre, c.Mascota.ClienteId,
            c.Mascota.Cliente.Nombre + " " + c.Mascota.Cliente.Apellido, c.DoctorId, c.Doctor.Nombre + " " + c.Doctor.Apellido,
            c.FechaHora, c.Motivo, c.Estado);
}
