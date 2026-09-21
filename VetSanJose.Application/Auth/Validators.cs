using System.Text.RegularExpressions;
using FluentValidation;

using VetSanJose.Shared.Auth;

namespace VetSanJose.Application.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RegistroClienteRequestValidator : AbstractValidator<RegistroClienteRequest>
{
    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÑÜáéíóúñü\s'-]+$", RegexOptions.Compiled);
    private static readonly Regex TelefonoRegex = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    public RegistroClienteRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100)
            .Matches(NombreRegex).WithMessage("Solo se permiten letras.");
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100)
            .Matches(NombreRegex).WithMessage("Solo se permiten letras.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Telefono).MaximumLength(30)
            .Matches(TelefonoRegex).WithMessage("Telefono debe contener solo digitos (7 a 15), con un '+' opcional al inicio.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
