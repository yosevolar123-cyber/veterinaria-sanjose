using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Mascotas;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Mascotas;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/mascotas")]
[Authorize]
public class MascotasController(
    IMascotasService mascotasService,
    IValidator<CrearMascotaRequest> crearValidator,
    IValidator<ActualizarMascotaRequest> actualizarValidator) : ControllerBase
{
    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<ActionResult<List<MascotaDto>>> MisMascotas(CancellationToken cancellationToken)
    {
        return Ok(await mascotasService.GetMisMascotasAsync(cancellationToken));
    }

    [HttpGet("cliente/{clienteId:long}")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<List<MascotaDto>>> PorCliente(long clienteId, CancellationToken cancellationToken)
    {
        return Ok(await mascotasService.GetPorClienteAsync(clienteId, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MascotaDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await mascotasService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<MascotaDto>> Crear(CrearMascotaRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var mascota = await mascotasService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = mascota.Id }, mascota);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<MascotaDto>> Actualizar(long id, ActualizarMascotaRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await mascotasService.ActualizarAsync(id, request, cancellationToken));
    }
}
