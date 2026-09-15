using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Categorias;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Categorias;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/categorias-producto")]
public class CategoriasProductoController(
    ICategoriasService categoriasService,
    IValidator<CrearCategoriaRequest> crearValidator,
    IValidator<ActualizarCategoriaRequest> actualizarValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoriaProductoDto>>> ObtenerTodas(CancellationToken cancellationToken)
    {
        return Ok(await categoriasService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<CategoriaProductoDto>> Crear(CrearCategoriaRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var categoria = await categoriasService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerTodas), categoria);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<CategoriaProductoDto>> Actualizar(long id, ActualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await categoriasService.ActualizarAsync(id, request, cancellationToken));
    }
}
