using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSanJose.Application.Dashboard;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Dashboard;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("doctor")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<ActionResult<DoctorDashboardDto>> Doctor(CancellationToken cancellationToken)
    {
        return Ok(await dashboardService.GetDoctorAsync(cancellationToken));
    }

    [HttpGet("secretaria")]
    [Authorize(Roles = Roles.Secretaria)]
    public async Task<ActionResult<SecretariaDashboardDto>> Secretaria(CancellationToken cancellationToken)
    {
        return Ok(await dashboardService.GetSecretariaAsync(cancellationToken));
    }

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<AdminDashboardDto>> Admin([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken cancellationToken)
    {
        return Ok(await dashboardService.GetAdminAsync(desde, hasta, cancellationToken));
    }
}
