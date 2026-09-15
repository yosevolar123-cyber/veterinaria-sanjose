using FluentValidation;

using VetSanJose.Shared.HistorialesMedicos;

namespace VetSanJose.Application.HistorialesMedicos;

public class InsumoUsadoRequestValidator : AbstractValidator<InsumoUsadoRequest>
{
    public InsumoUsadoRequestValidator()
    {
        RuleFor(x => x.ProductoId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}

public class CrearHistorialRequestValidator : AbstractValidator<CrearHistorialRequest>
{
    public CrearHistorialRequestValidator()
    {
        RuleFor(x => x.MascotaId).GreaterThan(0);
        RuleFor(x => x.Diagnostico).MaximumLength(2000);
        RuleFor(x => x.Tratamiento).MaximumLength(2000);
        RuleFor(x => x.Observaciones).MaximumLength(2000);
        RuleForEach(x => x.Insumos).SetValidator(new InsumoUsadoRequestValidator());
    }
}
