using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

public class UsuariosService(IAppDbContext db, ICurrentUser currentUser, IPasswordHasher passwordHasher) : IUsuariosService
{
    public async Task<UsuarioAdminDto> GetMeAsync(CancellationToken cancellationToken)
    {
        return Mapear(await BuscarAsync(currentUser.Id, cancellationToken));
    }

    public async Task<List<UsuarioAdminDto>> GetAllAsync(string? rol, bool? soloActivos, CancellationToken cancellationToken)
    {
        var query = db.Usuarios.AsQueryable();
        if (!string.IsNullOrWhiteSpace(rol))
        {
            query = query.Where(u => u.Rol == rol);
        }

        if (soloActivos == true)
        {
            query = query.Where(u => u.Activo);
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
        if (!currentUser.EsAdministrador && !(currentUser.EsSecretaria && request.Rol == Roles.Cliente))
        {
            throw new ForbiddenAppException("La secretaría sólo puede crear cuentas de tipo cliente.");
        }

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

        if (!request.Activo && usuario.Activo && usuario.Id == currentUser.Id)
        {
            throw new ConflictException("No puedes desactivar tu propia cuenta.");
        }

        if (!request.Activo || request.Rol != Roles.Administrador)
        {
            await VerificarQueNoEsElUltimoAdminAsync(usuario, cancellationToken);
        }

        if (request.Rol != usuario.Rol || (!request.Activo && usuario.Activo))
        {
            await RevocarSesionesAsync(usuario.Id, cancellationToken);
        }

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

    public async Task<UsuarioAdminDto> CambiarEstadoAsync(long id, bool activo, CancellationToken cancellationToken)
    {
        var usuario = await BuscarAsync(id, cancellationToken);

        if (usuario.Activo == activo)
        {
            return Mapear(usuario);
        }

        if (!activo)
        {
            if (usuario.Id == currentUser.Id)
            {
                throw new ConflictException("No puedes desactivar tu propia cuenta.");
            }

            await VerificarQueNoEsElUltimoAdminAsync(usuario, cancellationToken);
        }

        usuario.Activo = activo;

        if (!activo)
        {
            await RevocarSesionesAsync(usuario.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task<UsuarioAdminDto> CambiarRolAsync(long id, string rol, CancellationToken cancellationToken)
    {
        if (!Roles.Todos.Contains(rol))
        {
            throw new AppException($"Rol debe ser uno de: {string.Join(", ", Roles.Todos)}.");
        }

        var usuario = await BuscarAsync(id, cancellationToken);

        if (usuario.Rol == rol)
        {
            return Mapear(usuario);
        }

        if (rol != Roles.Administrador)
        {
            await VerificarQueNoEsElUltimoAdminAsync(usuario, cancellationToken);
        }

        usuario.Rol = rol;

        // El cambio se aplica recién en el próximo login: se revocan los refresh tokens para que
        // la sesión abierta no siga renovando un access token con el rol anterior.
        await RevocarSesionesAsync(usuario.Id, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    private async Task VerificarQueNoEsElUltimoAdminAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        if (usuario.Rol != Roles.Administrador || !usuario.Activo)
        {
            return;
        }

        var otrosAdminsActivos = await db.Usuarios
            .CountAsync(u => u.Rol == Roles.Administrador && u.Activo && u.Id != usuario.Id, cancellationToken);

        if (otrosAdminsActivos == 0)
        {
            throw new ConflictException(
                "Es el único administrador activo del sistema. Asigná otro administrador antes de desactivarlo o cambiarle el rol.");
        }
    }

    private async Task RevocarSesionesAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.UsuarioId == usuarioId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }
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
