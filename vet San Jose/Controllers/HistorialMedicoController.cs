using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.HistorialesMedicos;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.HistorialesMedicos;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/historial-medico")]
[Authorize]
public class HistorialMedicoController(
    IHistorialMedicoService historialService,
    IValidator<CrearHistorialRequest> crearValidator) : ControllerBase
{
    [HttpGet("mascota/{mascotaId:long}")]
    public async Task<ActionResult<List<HistorialMedicoDto>>> PorMascota(long mascotaId, CancellationToken cancellationToken)
    {
        return Ok(await historialService.GetPorMascotaAsync(mascotaId, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<HistorialMedicoDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await historialService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<HistorialMedicoDto>> Crear(CrearHistorialRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var historial = await historialService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = historial.Id }, historial);
    }
}
