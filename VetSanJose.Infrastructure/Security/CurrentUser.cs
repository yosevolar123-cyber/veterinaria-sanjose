using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Common;

namespace VetSanJose.Infrastructure.Security;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public long Id
    {
        get
        {
            var value = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(value, out var id) ? id : 0;
        }
    }

    public string Rol => Principal?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public bool EsAdministrador => Rol == Roles.Administrador;
    public bool EsSecretaria => Rol == Roles.Secretaria;
    public bool EsDoctor => Rol == Roles.Doctor;
    public bool EsCliente => Rol == Roles.Cliente;
}
