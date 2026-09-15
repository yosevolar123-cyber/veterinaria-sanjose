using FluentValidation;

using VetSanJose.Shared.Ventas;

namespace VetSanJose.Application.Ventas;

public class ItemCarritoRequestValidator : AbstractValidator<ItemCarritoRequest>
{
    public ItemCarritoRequestValidator()
    {
        RuleFor(x => x.ProductoId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}

public class CrearVentaRequestValidator : AbstractValidator<CrearVentaRequest>
{
    public CrearVentaRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("El carrito no puede estar vacío.");
        RuleForEach(x => x.Items).SetValidator(new ItemCarritoRequestValidator());
    }
}
