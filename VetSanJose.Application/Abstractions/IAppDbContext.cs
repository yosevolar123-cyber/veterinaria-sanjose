using Microsoft.EntityFrameworkCore;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Mascota> Mascotas { get; }
    DbSet<CategoriaProducto> CategoriasProducto { get; }
    DbSet<Proveedor> Proveedores { get; }
    DbSet<Producto> Productos { get; }
    DbSet<Cita> Citas { get; }
    DbSet<HistorialMedico> HistorialesMedicos { get; }
    DbSet<UsoInsumo> UsosInsumo { get; }
    DbSet<Venta> Ventas { get; }
    DbSet<DetalleVenta> DetallesVenta { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
