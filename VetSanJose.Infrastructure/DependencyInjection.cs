using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VetSanJose.Application.Abstractions;
using VetSanJose.Infrastructure.Persistence;
using VetSanJose.Infrastructure.Security;
using VetSanJose.Infrastructure.Storage;

namespace VetSanJose.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = PrimerValorConfigurado(
                configuration.GetConnectionString("Supabase"),
                configuration["ConnectionStrings:Supabase"],
                Environment.GetEnvironmentVariable("ConnectionStrings__Supabase"))
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'Supabase'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Storage es opcional para arrancar: si falta configuración la API sigue en pie y el error
        // se reporta con un mensaje claro recién al intentar subir una imagen (ver SupabaseStorageService).
        var supabaseUrl = PrimerValorConfigurado(
            configuration["Supabase:Url"],
            Environment.GetEnvironmentVariable("Supabase__Url"));

        var supabaseServiceKey = PrimerValorConfigurado(
            configuration["Supabase:ServiceKey"],
            Environment.GetEnvironmentVariable("Supabase__ServiceKey"));

        Uri? supabaseBaseUri = null;
        if (supabaseUrl is not null)
        {
            Uri.TryCreate(supabaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out supabaseBaseUri);
        }

        services.Configure<SupabaseStorageSettings>(options =>
        {
            configuration.GetSection(SupabaseStorageSettings.SectionName).Bind(options);
            options.Url = supabaseUrl ?? "";
            options.ServiceKey = supabaseServiceKey ?? "";
        });

        services.AddHttpClient<IStorageService, SupabaseStorageService>((sp, client) =>
        {
            if (supabaseBaseUri is not null)
            {
                client.BaseAddress = supabaseBaseUri;
            }

            if (supabaseServiceKey is not null)
            {
                client.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", supabaseServiceKey);
            }
        });

        return services;
    }

    // Los archivos appsettings.json del repositorio traen los valores de Supabase como cadenas
    // vacías (placeholders). Con '??' esas cadenas vacías ganaban sobre las variables de entorno y
    // terminaban en `new Uri("/")`, que revienta al subir una imagen; por eso se descartan aquí.
    private static string? PrimerValorConfigurado(params string?[] valores) =>
        valores.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
