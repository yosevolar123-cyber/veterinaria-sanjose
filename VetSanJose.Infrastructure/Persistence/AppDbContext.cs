using Microsoft.EntityFrameworkCore;
using VetSanJose.Application.Abstractions;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<HistorialMedico> HistorialesMedicos => Set<HistorialMedico>();
    public DbSet<UsoInsumo> UsosInsumo => Set<UsoInsumo>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
