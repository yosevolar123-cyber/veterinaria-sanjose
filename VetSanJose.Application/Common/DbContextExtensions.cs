using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;

namespace VetSanJose.Application.Common;

public static class DbContextExtensions
{
    public static async Task SaveChangesTraduciendoErroresAsync(this IAppDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Stock insuficiente", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mensaje = ex.InnerException!.Message;
            var indiceInicio = mensaje.IndexOf("Stock insuficiente", StringComparison.OrdinalIgnoreCase);
            throw new AppException(mensaje[indiceInicio..]);
        }
    }
}
