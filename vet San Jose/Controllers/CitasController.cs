using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Citas;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Citas;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/citas")]
[Authorize]
public class CitasController(
    ICitasService citasService,
    IValidator<CrearCitaRequest> crearValidator,
    IValidator<ActualizarCitaRequest> actualizarValidator) : ControllerBase
{
    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<ActionResult<List<CitaDto>>> MisCitas(CancellationToken cancellationToken)
    {
        return Ok(await citasService.GetMisCitasAsync(cancellationToken));
    }

    [HttpGet("agenda")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<List<CitaDto>>> Agenda([FromQuery] long? doctorId, [FromQuery] DateOnly? fecha, CancellationToken cancellationToken)
    {
        return Ok(await citasService.GetAgendaAsync(doctorId, fecha, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CitaDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await citasService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<CitaDto>> Crear(CrearCitaRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var cita = await citasService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cita.Id }, cita);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<CitaDto>> Actualizar(long id, ActualizarCitaRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await citasService.ActualizarAsync(id, request, cancellationToken));
    }
}
