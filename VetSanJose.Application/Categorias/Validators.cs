using FluentValidation;

using VetSanJose.Shared.Categorias;

namespace VetSanJose.Application.Categorias;

public class CrearCategoriaRequestValidator : AbstractValidator<CrearCategoriaRequest>
{
    public CrearCategoriaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Descripcion).MaximumLength(500);
    }
}

public class ActualizarCategoriaRequestValidator : AbstractValidator<ActualizarCategoriaRequest>
{
    public ActualizarCategoriaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Descripcion).MaximumLength(500);
    }
}
