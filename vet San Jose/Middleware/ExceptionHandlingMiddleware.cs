using Microsoft.AspNetCore.Mvc;
using VetSanJose.Application.Common;

namespace vet_San_Jose.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning(ex, "Error de aplicación manejado: {Message}", ex.Message);
            await EscribirProblemaAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error no controlado procesando {Method} {Path}. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}. InnerException: {InnerExceptionMessage}. StackTrace: {StackTrace}",
                context.Request.Method,
                context.Request.Path,
                ex.GetType().FullName,
                ex.Message,
                ex.InnerException?.Message,
                ex.StackTrace);

            await EscribirProblemaAsync(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Intente nuevamente más tarde.");
        }
    }

    private static async Task EscribirProblemaAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.For(statusCode),
            Detail = detail,
            Instance = context.Request.Path,
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

internal static class ReasonPhrases
{
    public static string For(int statusCode) => statusCode switch
    {
        400 => "Solicitud inválida",
        401 => "No autenticado",
        403 => "No autorizado",
        404 => "No encontrado",
        409 => "Conflicto",
        502 => "Error del servicio de almacenamiento",
        503 => "Servicio no disponible",
        _ => "Error interno del servidor",
    };
}
