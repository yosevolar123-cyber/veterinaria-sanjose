using System.Text.RegularExpressions;
using FluentValidation;
using VetSanJose.Domain.Common;

using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

internal static class UsuarioValidationRules
{
    public static readonly Regex TelefonoRegex = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);
    public const string TelefonoMensaje = "Telefono debe contener solo digitos (7 a 15), con un '+' opcional al inicio.";
}

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
        RuleFor(x => x.Telefono).MaximumLength(30)
            .Matches(UsuarioValidationRules.TelefonoRegex).WithMessage(UsuarioValidationRules.TelefonoMensaje)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
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
        RuleFor(x => x.Telefono).MaximumLength(30)
            .Matches(UsuarioValidationRules.TelefonoRegex).WithMessage(UsuarioValidationRules.TelefonoMensaje)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
        RuleFor(x => x.Especialidad).MaximumLength(150);
        RuleFor(x => x.Matricula).MaximumLength(50);
    }
}
