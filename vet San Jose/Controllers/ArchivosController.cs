using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Common;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/archivos")]
[Authorize]
public class ArchivosController(IStorageService storageService) : ControllerBase
{
    private const long TamanoMaximoBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> ContentTypesPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp",
    };
    private static readonly Dictionary<string, string> ExtensionPorContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };
    private static readonly Dictionary<string, string[]> RolesPorCarpeta = new(StringComparer.OrdinalIgnoreCase)
    {
        ["productos"] = [Roles.Administrador, Roles.Secretaria],
        ["mascotas"] = [Roles.Cliente, Roles.Administrador],
    };

    [HttpPost("imagenes")]
    public async Task<ActionResult<SubirImagenResponse>> SubirImagen(
        [FromForm] string? carpeta, [FromForm] IFormFile archivo, CancellationToken cancellationToken)
    {
        if (carpeta is null || !RolesPorCarpeta.TryGetValue(carpeta, out var rolesPermitidos))
        {
            return BadRequest(new { mensaje = "Carpeta no válida." });
        }

        if (!User.IsInRole(Roles.Administrador) && !rolesPermitidos.Any(User.IsInRole))
        {
            return Forbid();
        }

        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe adjuntar un archivo." });
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            return BadRequest(new { mensaje = "El archivo supera el tamaño máximo permitido (5 MB)." });
        }

        var contentType = archivo.ContentType;
        if (string.IsNullOrEmpty(contentType) || !ContentTypesPermitidos.Contains(contentType))
        {
            return BadRequest(new { mensaje = "Formato no soportado. Use JPEG, PNG o WEBP." });
        }

        var extension = ExtensionPorContentType[contentType];
        var nombreArchivo = $"{Guid.NewGuid():N}.{extension}";

        await using var stream = archivo.OpenReadStream();
        var url = await storageService.SubirArchivoAsync(carpeta.ToLowerInvariant(), nombreArchivo, contentType, stream, cancellationToken);

        return Ok(new SubirImagenResponse(url));
    }
}

public record SubirImagenResponse(string Url);
