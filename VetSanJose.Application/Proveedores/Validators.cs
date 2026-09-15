using FluentValidation;

using VetSanJose.Shared.Proveedores;

namespace VetSanJose.Application.Proveedores;

public class CrearProveedorRequestValidator : AbstractValidator<CrearProveedorRequest>
{
    public CrearProveedorRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactoNombre).MaximumLength(150);
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Direccion).MaximumLength(300);
    }
}

public class ActualizarProveedorRequestValidator : AbstractValidator<ActualizarProveedorRequest>
{
    public ActualizarProveedorRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactoNombre).MaximumLength(150);
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Direccion).MaximumLength(300);
    }
}
