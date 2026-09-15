using FluentValidation;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Productos;

namespace VetSanJose.Application.Productos;

public class CrearProductoRequestValidator : AbstractValidator<CrearProductoRequest>
{
    public CrearProductoRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descripcion).MaximumLength(1000);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImagenUrl).MaximumLength(2048);
        RuleFor(x => x.Tipo).Must(t => TiposProducto.Todos.Contains(t))
            .WithMessage($"Tipo debe ser uno de: {string.Join(", ", TiposProducto.Todos)}.");
    }
}

public class ActualizarProductoRequestValidator : AbstractValidator<ActualizarProductoRequest>
{
    public ActualizarProductoRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descripcion).MaximumLength(1000);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImagenUrl).MaximumLength(2048);
        RuleFor(x => x.Tipo).Must(t => TiposProducto.Todos.Contains(t))
            .WithMessage($"Tipo debe ser uno de: {string.Join(", ", TiposProducto.Todos)}.");
    }
}

public class AjustarStockRequestValidator : AbstractValidator<AjustarStockRequest>
{
    public AjustarStockRequestValidator()
    {
        RuleFor(x => x.Cantidad).NotEqual(0);
    }
}
