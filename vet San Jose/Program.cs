using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using vet_San_Jose.Middleware;
using VetSanJose.Application;
using VetSanJose.Application.Abstractions;
using VetSanJose.Infrastructure;
using VetSanJose.Infrastructure.Persistence;
using VetSanJose.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

JwtSettings jwtSettings;
byte[] jwtSigningKeyBytes;
try
{
    jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException(
            $"Falta la sección de configuración '{JwtSettings.SectionName}'. " +
            "Defina las variables de entorno Jwt__Issuer, Jwt__Audience y Jwt__SigningKey.");

    if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    {
        throw new InvalidOperationException("Falta la variable de entorno 'Jwt__Issuer'.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    {
        throw new InvalidOperationException("Falta la variable de entorno 'Jwt__Audience'.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey))
    {
        throw new InvalidOperationException("Falta la variable de entorno 'Jwt__SigningKey'.");
    }

    try
    {
        jwtSigningKeyBytes = Convert.FromBase64String(jwtSettings.SigningKey);
    }
    catch (FormatException ex)
    {
        throw new InvalidOperationException(
            "La variable de entorno 'Jwt__SigningKey' no contiene un valor Base64 válido.", ex);
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error al cargar la configuración de Jwt: {Message}", ex.Message);
    throw;
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

const string corsPolicyName = "FrontendPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Veterinaria San José API", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo 'Bearer '.",
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    var bearerSchemeReference = new OpenApiSecuritySchemeReference("Bearer");
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement { { bearerSchemeReference, [] } });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    try
    {
        await DbSeeder.SeedAdministradorAsync(db, passwordHasher, app.Configuration);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "No se pudo ejecutar el seed del administrador (¿la base de datos está disponible?).");
    }
}

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Habilitar Swagger independientemente del entorno (Development o Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Veterinaria San Jose API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
