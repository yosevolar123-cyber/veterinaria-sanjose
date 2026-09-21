using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VetSanJose.Application.Auth;
using VetSanJose.Application.Categorias;
using VetSanJose.Application.Citas;
using VetSanJose.Application.Dashboard;
using VetSanJose.Application.HistorialesMedicos;
using VetSanJose.Application.Mascotas;
using VetSanJose.Application.Productos;
using VetSanJose.Application.Proveedores;
using VetSanJose.Application.Reportes;
using VetSanJose.Application.Usuarios;
using VetSanJose.Application.Ventas;

namespace VetSanJose.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMascotasService, MascotasService>();
        services.AddScoped<ICategoriasService, CategoriasService>();
        services.AddScoped<IProductosService, ProductosService>();
        services.AddScoped<ICitasService, CitasService>();
        services.AddScoped<IHistorialMedicoService, HistorialMedicoService>();
        services.AddScoped<IProveedoresService, ProveedoresService>();
        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IVentasService, VentasService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportesService, ReportesService>();

        return services;
    }
}
