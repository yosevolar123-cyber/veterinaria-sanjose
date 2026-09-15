using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vet_San_Jose.Extensions;
using VetSanJose.Application.Productos;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Productos;

namespace vet_San_Jose.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(
    IProductosService productosService,
    IValidator<CrearProductoRequest> crearValidator,
    IValidator<ActualizarProductoRequest> actualizarValidator,
    IValidator<AjustarStockRequest> ajustarStockValidator) : ControllerBase
{
    [HttpGet("tienda")]
    public async Task<ActionResult<List<ProductoTiendaDto>>> Tienda([FromQuery] long? categoriaId, CancellationToken cancellationToken)
    {
        return Ok(await productosService.GetTiendaAsync(categoriaId, cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<List<ProductoDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await productosService.GetAllAsync(cancellationToken));
    }

    [HttpGet("stock-bajo")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<List<ProductoDto>>> StockBajo(
        [FromQuery] int umbral = InventarioConfig.UmbralStockBajoPorDefecto, CancellationToken cancellationToken = default)
    {
        return Ok(await productosService.GetStockBajoAsync(umbral, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Secretaria},{Roles.Administrador}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(long id, CancellationToken cancellationToken)
    {
        return Ok(await productosService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Secretaria}")]
    public async Task<ActionResult<ProductoDto>> Crear(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        if (await crearValidator.ValidarAsync(request) is { } error) return error;

        var producto = await productosService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Secretaria}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(long id, ActualizarProductoRequest request, CancellationToken cancellationToken)
    {
        if (await actualizarValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await productosService.ActualizarAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:long}/stock")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Secretaria}")]
    public async Task<ActionResult<ProductoDto>> AjustarStock(long id, AjustarStockRequest request, CancellationToken cancellationToken)
    {
        if (await ajustarStockValidator.ValidarAsync(request) is { } error) return error;

        return Ok(await productosService.AjustarStockAsync(id, request, cancellationToken));
    }
}
