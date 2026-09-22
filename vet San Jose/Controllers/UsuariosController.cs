using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Usuarios;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Usuarios;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController(
    IUsuariosService usuariosService,
    IValidator<CrearUsuarioRequest> crearValidator,
    IValidator<ActualizarUsuarioRequest> actualizarValidator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioAdminDto>> Yo(CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.GetMeAsync(cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<List<UsuarioAdminDto>>> ObtenerTodos(
        [FromQuery] string? rol, [FromQuery] bool? soloActivos, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.GetAllAsync(rol, soloActivos, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<UsuarioAdminDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<UsuarioAdminDto>> Crear(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var usuario = await usuariosService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<UsuarioAdminDto>> Actualizar(long id, ActualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await usuariosService.ActualizarAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:long}/estado")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<UsuarioAdminDto>> CambiarEstado(
        long id, CambiarEstadoUsuarioRequest request, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.CambiarEstadoAsync(id, request.Activo, cancellationToken));
    }

    [HttpPatch("{id:long}/rol")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<UsuarioAdminDto>> CambiarRol(
        long id, CambiarRolUsuarioRequest request, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.CambiarRolAsync(id, request.Rol, cancellationToken));
    }
}
