using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSanJose.Application.Reportes;
using VetSanJose.Domain.Common;
using VetSanJose.Shared.Reportes;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = Roles.Administrador)]
public class ReportesController(IReportesService reportesService) : ControllerBase
{
    [HttpGet("financiero")]
    public async Task<ActionResult<ReporteFinancieroDto>> Financiero(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetReporteFinancieroAsync(desde, hasta, cancellationToken));
    }

    [HttpGet("negocio-mes")]
    public async Task<ActionResult<ReporteNegocioMesDto>> NegocioMes(
        [FromQuery] int? anio, [FromQuery] int? mes, CancellationToken cancellationToken)
    {
        return Ok(await reportesService.GetReporteNegocioMesAsync(anio, mes, cancellationToken));
    }

    [HttpGet("financiero/pdf")]
    public async Task<IActionResult> FinancieroPdf(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken cancellationToken)
    {
        var pdf = await reportesService.GetReporteFinancieroPdfAsync(desde, hasta, cancellationToken);
        var nombreArchivo = $"reporte-financiero-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf";
        return File(pdf, "application/pdf", nombreArchivo);
    }
}
