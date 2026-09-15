using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

using VetSanJose.Shared.Auth;

namespace VetSanJose.Application.Auth;

public class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (usuario is null || !passwordHasher.Verify(request.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedAppException("Credenciales inválidas.");
        }

        if (!usuario.Activo)
        {
            throw new ForbiddenAppException("El usuario está desactivado.");
        }

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<AuthResponse> RegistrarClienteAsync(RegistroClienteRequest request, CancellationToken cancellationToken)
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
            Rol = Roles.Cliente,
            Telefono = request.Telefono,
            Activo = true,
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(cancellationToken);

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);

        var tokenActual = await db.RefreshTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (tokenActual is null || !tokenActual.IsActive)
        {
            throw new UnauthorizedAppException("El token de actualización es inválido o ha expirado.");
        }

        if (!tokenActual.Usuario.Activo)
        {
            throw new ForbiddenAppException("El usuario está desactivado.");
        }

        tokenActual.RevokedAt = DateTimeOffset.UtcNow;

        return await EmitirTokensAsync(tokenActual.Usuario, cancellationToken);
    }

    private async Task<AuthResponse> EmitirTokensAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var accessToken = tokenService.GenerateAccessToken(usuario);
        var refreshTokenRaw = tokenService.GenerateRefreshToken();
        var expiresAt = DateTimeOffset.UtcNow.Add(tokenService.AccessTokenLifetime);

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenService.HashToken(refreshTokenRaw),
            ExpiresAt = DateTimeOffset.UtcNow.Add(tokenService.RefreshTokenLifetime),
        });

        await db.SaveChangesAsync(cancellationToken);

        var usuarioDto = new UsuarioDto(
            usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email,
            usuario.Rol, usuario.Telefono, usuario.Activo);

        return new AuthResponse(accessToken, refreshTokenRaw, expiresAt, usuarioDto);
    }
}
