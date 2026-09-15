using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace vet_San_Jose.Extensions;

public static class ValidationExtensions
{
    public static async Task<ActionResult?> ValidarAsync<T>(this IValidator<T> validator, T instancia)
    {
        var resultado = await validator.ValidateAsync(instancia);
        if (resultado.IsValid)
        {
            return null;
        }

        var errores = resultado.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ValidationProblemDetails(errores));
    }
}
