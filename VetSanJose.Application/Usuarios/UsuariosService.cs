using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

public class UsuariosService(IAppDbContext db, ICurrentUser currentUser, IPasswordHasher passwordHasher) : IUsuariosService
{
    public async Task<UsuarioAdminDto> GetMeAsync(CancellationToken cancellationToken)
    {
        return Mapear(await BuscarAsync(currentUser.Id, cancellationToken));
    }

    public async Task<List<UsuarioAdminDto>> GetAllAsync(string? rol, CancellationToken cancellationToken)
    {
        var query = db.Usuarios.AsQueryable();
        if (!string.IsNullOrWhiteSpace(rol))
        {
            query = query.Where(u => u.Rol == rol);
        }

        return await query
            .OrderBy(u => u.Nombre)
            .Select(MapExpression)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    public async Task<UsuarioAdminDto> CrearAsync(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        var existe = await db.Usuarios.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (existe)
        {
            throw new ConflictException("Ya existe un usuario registrado con ese correo.");
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Rol = request.Rol,
            Telefono = request.Telefono,
            Especialidad = request.Especialidad,
            Matricula = request.Matricula,
            Activo = true,
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task<UsuarioAdminDto> ActualizarAsync(long id, ActualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await BuscarAsync(id, cancellationToken);

        usuario.Nombre = request.Nombre;
        usuario.Apellido = request.Apellido;
        usuario.Rol = request.Rol;
        usuario.Telefono = request.Telefono;
        usuario.Especialidad = request.Especialidad;
        usuario.Matricula = request.Matricula;
        usuario.Activo = request.Activo;

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task EliminarAsync(long id, CancellationToken cancellationToken)
    {
        var usuario = await BuscarAsync(id, cancellationToken);

        if (usuario.Id == currentUser.Id)
        {
            throw new ConflictException("No puedes eliminar tu propio usuario.");
        }

        var tieneRegistrosAsociados =
            await db.Mascotas.AnyAsync(m => m.ClienteId == id, cancellationToken) ||
            await db.Citas.AnyAsync(c => c.DoctorId == id || c.CreadoPor == id, cancellationToken) ||
            await db.HistorialesMedicos.AnyAsync(h => h.DoctorId == id, cancellationToken) ||
            await db.Ventas.AnyAsync(v => v.ClienteId == id, cancellationToken);

        if (tieneRegistrosAsociados)
        {
            throw new ConflictException(
                "No se puede eliminar este usuario porque tiene mascotas, citas, ventas o historial médico asociados. Desactívalo en su lugar.");
        }

        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> BuscarAsync(long id, CancellationToken cancellationToken)
    {
        return await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");
    }

    private static UsuarioAdminDto Mapear(Usuario u) =>
        new(u.Id, u.Nombre, u.Apellido, u.Email, u.Rol, u.Telefono, u.Activo, u.Especialidad, u.Matricula);

    private static readonly System.Linq.Expressions.Expression<Func<Usuario, UsuarioAdminDto>> MapExpression = u =>
        new UsuarioAdminDto(u.Id, u.Nombre, u.Apellido, u.Email, u.Rol, u.Telefono, u.Activo, u.Especialidad, u.Matricula);
}
