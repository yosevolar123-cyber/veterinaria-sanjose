using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Proveedores;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Proveedores;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
public class ProveedoresController(
    IProveedoresService proveedoresService,
    IValidator<CrearProveedorRequest> crearValidator,
    IValidator<ActualizarProveedorRequest> actualizarValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProveedorDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await proveedoresService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProveedorDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await proveedoresService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ProveedorDto>> Crear(CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var proveedor = await proveedoresService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ProveedorDto>> Actualizar(long id, ActualizarProveedorRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await proveedoresService.ActualizarAsync(id, request, cancellationToken));
    }
}
