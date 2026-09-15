using FluentValidation;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

public class CrearUsuarioRequestValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Rol).Must(r => Roles.Todos.Contains(r))
            .WithMessage($"Rol debe ser uno de: {string.Join(", ", Roles.Todos)}.");
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Especialidad).MaximumLength(150);
        RuleFor(x => x.Matricula).MaximumLength(50);
    }
}

public class ActualizarUsuarioRequestValidator : AbstractValidator<ActualizarUsuarioRequest>
{
    public ActualizarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rol).Must(r => Roles.Todos.Contains(r))
            .WithMessage($"Rol debe ser uno de: {string.Join(", ", Roles.Todos)}.");
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Especialidad).MaximumLength(150);
        RuleFor(x => x.Matricula).MaximumLength(50);
    }
}
