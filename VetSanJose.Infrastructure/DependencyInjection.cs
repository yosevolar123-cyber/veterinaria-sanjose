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
        var connectionString = configuration.GetConnectionString("Supabase")
            ?? configuration["ConnectionStrings:Supabase"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'Supabase'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var supabaseUrl = configuration["Supabase:Url"]
            ?? Environment.GetEnvironmentVariable("Supabase__Url")
            ?? throw new InvalidOperationException("No se encontró la configuración 'Supabase:Url'.");
        var supabaseServiceKey = configuration["Supabase:ServiceKey"]
            ?? Environment.GetEnvironmentVariable("Supabase__ServiceKey")
            ?? throw new InvalidOperationException("No se encontró la configuración 'Supabase:ServiceKey'.");

        services.Configure<SupabaseStorageSettings>(options =>
        {
            configuration.GetSection(SupabaseStorageSettings.SectionName).Bind(options);
            options.Url = supabaseUrl;
            options.ServiceKey = supabaseServiceKey;
        });

        services.AddHttpClient<IStorageService, SupabaseStorageService>((sp, client) =>
        {
            client.BaseAddress = new Uri(supabaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", supabaseServiceKey);
        });

        return services;
    }
}
