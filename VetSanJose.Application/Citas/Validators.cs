using FluentValidation;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Citas;

namespace VetSanJose.Application.Citas;

public class CrearCitaRequestValidator : AbstractValidator<CrearCitaRequest>
{
    public CrearCitaRequestValidator()
    {
        RuleFor(x => x.MascotaId).GreaterThan(0);
        RuleFor(x => x.FechaHora).GreaterThan(DateTimeOffset.UtcNow).WithMessage("La fecha de la cita debe ser futura.");
        RuleFor(x => x.Motivo).MaximumLength(500);
    }
}

public class ActualizarCitaRequestValidator : AbstractValidator<ActualizarCitaRequest>
{
    private static readonly string[] EstadosValidos =
        [EstadosCita.Pendiente, EstadosCita.Confirmada, EstadosCita.Completada, EstadosCita.Cancelada];

    public ActualizarCitaRequestValidator()
    {
        RuleFor(x => x.FechaHora).NotEmpty();
        RuleFor(x => x.Motivo).MaximumLength(500);
        RuleFor(x => x.Estado).Must(e => EstadosValidos.Contains(e))
            .WithMessage($"Estado debe ser uno de: {string.Join(", ", EstadosValidos)}.");
    }
}
