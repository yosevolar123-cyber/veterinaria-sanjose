using FluentValidation;

using VetSanJose.Shared.Mascotas;

namespace VetSanJose.Application.Mascotas;

public class CrearMascotaRequestValidator : AbstractValidator<CrearMascotaRequest>
{
    public CrearMascotaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Especie).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Raza).MaximumLength(100);
        RuleFor(x => x.Sexo).Must(s => s is null || s is "macho" or "hembra")
            .WithMessage("Sexo debe ser 'macho' o 'hembra'.");
        RuleFor(x => x.Peso).GreaterThan(0).When(x => x.Peso.HasValue);
    }
}

public class ActualizarMascotaRequestValidator : AbstractValidator<ActualizarMascotaRequest>
{
    public ActualizarMascotaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Especie).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Raza).MaximumLength(100);
        RuleFor(x => x.Sexo).Must(s => s is null || s is "macho" or "hembra")
            .WithMessage("Sexo debe ser 'macho' o 'hembra'.");
        RuleFor(x => x.Peso).GreaterThan(0).When(x => x.Peso.HasValue);
    }
}
