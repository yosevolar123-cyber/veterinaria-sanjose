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
    public async Task<ActionResult<List<UsuarioAdminDto>>> ObtenerTodos([FromQuery] string? rol, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.GetAllAsync(rol, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<UsuarioAdminDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await usuariosService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
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

    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(long id, CancellationToken cancellationToken)
    {
        await usuariosService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
