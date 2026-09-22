using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Mascotas;

namespace VetSanJose.Application.Mascotas;

public class MascotasService(IAppDbContext db, ICurrentUser currentUser) : IMascotasService
{
    public async Task<List<MascotaDto>> GetMisMascotasAsync(CancellationToken cancellationToken)
    {
        return await db.Mascotas
            .Where(m => m.ClienteId == currentUser.Id)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MascotaDto>> GetPorClienteAsync(long clienteId, CancellationToken cancellationToken)
    {
        if (currentUser.EsCliente && currentUser.Id != clienteId)
        {
            throw new ForbiddenAppException("No puede consultar mascotas de otro cliente.");
        }

        return await db.Mascotas
            .Where(m => m.ClienteId == clienteId)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<MascotaDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var mascota = await BuscarAsync(id, cancellationToken);
        VerificarPropiedad(mascota);
        return MapToDto(mascota);
    }

    public async Task<MascotaDto> CrearAsync(CrearMascotaRequest request, CancellationToken cancellationToken)
    {
        long clienteId;
        if (currentUser.EsCliente)
        {
            clienteId = currentUser.Id;
        }
        else if (currentUser.EsAdministrador || currentUser.EsSecretaria)
        {
            if (request.ClienteId is null)
            {
                throw new AppException("Debe especificar el cliente propietario de la mascota.");
            }

            clienteId = request.ClienteId.Value;
        }
        else
        {
            throw new ForbiddenAppException("No tiene permisos para registrar mascotas.");
        }

        var mascota = new Mascota
        {
            ClienteId = clienteId,
            Nombre = request.Nombre,
            Especie = request.Especie,
            Raza = request.Raza,
            FechaNacimiento = request.FechaNacimiento,
            Sexo = request.Sexo,
            Peso = request.Peso,
            FotoUrl = request.FotoUrl,
            Activo = true,
        };

        db.Mascotas.Add(mascota);
        await db.SaveChangesAsync(cancellationToken);

        return MapToDto(mascota);
    }

    public async Task<MascotaDto> ActualizarAsync(long id, ActualizarMascotaRequest request, CancellationToken cancellationToken)
    {
        var mascota = await BuscarAsync(id, cancellationToken);
        VerificarPropiedad(mascota);

        if (!currentUser.EsCliente && !currentUser.EsAdministrador && !currentUser.EsSecretaria)
        {
            throw new ForbiddenAppException("No tiene permisos para editar mascotas.");
        }

        mascota.Nombre = request.Nombre;
        mascota.Especie = request.Especie;
        mascota.Raza = request.Raza;
        mascota.FechaNacimiento = request.FechaNacimiento;
        mascota.Sexo = request.Sexo;
        mascota.Peso = request.Peso;
        mascota.FotoUrl = request.FotoUrl;

        if (currentUser.EsAdministrador)
        {
            mascota.Activo = request.Activo;
        }

        await db.SaveChangesAsync(cancellationToken);

        return MapToDto(mascota);
    }

    private async Task<Mascota> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await db.Mascotas.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Mascota no encontrada.");
    }

    private void VerificarPropiedad(Mascota mascota)
    {
        if (currentUser.EsCliente && mascota.ClienteId != currentUser.Id)
        {
            throw new ForbiddenAppException("No puede acceder a mascotas de otro cliente.");
        }
    }

    private static readonly System.Linq.Expressions.Expression<Func<Mascota, MascotaDto>> MapExpression =
        m => new MascotaDto(m.Id, m.ClienteId, m.Nombre, m.Especie, m.Raza, m.FechaNacimiento, m.Sexo, m.Peso, m.FotoUrl, m.Activo);

    private static MascotaDto MapToDto(Mascota m) =>
        new(m.Id, m.ClienteId, m.Nombre, m.Especie, m.Raza, m.FechaNacimiento, m.Sexo, m.Peso, m.FotoUrl, m.Activo);
}
