using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Auth;

using VetSanJose.Shared.Auth;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegistroClienteRequest> registroValidator,
    IValidator<RefreshRequest> refreshValidator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (await loginValidator.ValidarAsync(request) is { } error) return error;

        var respuesta = await authService.LoginAsync(request, cancellationToken);
        return Ok(respuesta);
    }

    [HttpPost("registro-cliente")]
    public async Task<ActionResult<AuthResponse>> RegistroCliente(RegistroClienteRequest request, CancellationToken cancellationToken)
    {
        if (await registroValidator.ValidarAsync(request) is { } error) return error;

        var respuesta = await authService.RegistrarClienteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Login), respuesta);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        if (await refreshValidator.ValidarAsync(request) is { } error) return error;

        var respuesta = await authService.RefreshAsync(request, cancellationToken);
        return Ok(respuesta);
    }
}
