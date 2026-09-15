using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Ventas;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Ventas;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController(
    IVentasService ventasService,
    IValidator<CrearVentaRequest> crearValidator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<ActionResult<VentaDto>> Crear(CrearVentaRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var venta = await ventasService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
    }

    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<ActionResult<List<VentaDto>>> MisVentas(CancellationToken cancellationToken)
    {
        return Ok(await ventasService.GetMisVentasAsync(cancellationToken));
    }

    [HttpGet("cliente/{clienteId:long}")]
    public async Task<ActionResult<List<VentaDto>>> PorCliente(long clienteId, CancellationToken cancellationToken)
    {
        return Ok(await ventasService.GetPorClienteAsync(clienteId, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<VentaDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await ventasService.GetByIdAsync(id, cancellationToken));
    }
}
