using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Common;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAdministradorAsync(AppDbContext db, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        var existeAdmin = await db.Usuarios.AnyAsync(u => u.Rol == Roles.Administrador);
        if (existeAdmin)
        {
            return;
        }

        var email = configuration["SeedAdmin:Email"] ?? "admin@vetsanjose.com";
        var password = configuration["SeedAdmin:Password"] ?? "CambiarInmediatamente123!";

        db.Usuarios.Add(new Usuario
        {
            Nombre = "Administrador",
            Apellido = "Principal",
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            Rol = Roles.Administrador,
            Activo = true,
        });

        await db.SaveChangesAsync();
    }
}
