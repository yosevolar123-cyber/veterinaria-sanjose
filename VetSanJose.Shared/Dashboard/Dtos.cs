namespace VetSanJose.Shared.Dashboard;

public record DoctorDashboardDto(int CitasHoy, DateTimeOffset? ProximaCita, int ProductosStockBajo);

public record SecretariaDashboardDto(int CitasHoy, int CitasPendientes);

public record CitasPorDoctorItem(long DoctorId, string DoctorNombre, int Cantidad);

public record ProductoStockBajoItem(long ProductoId, string Nombre, int Stock);

public record AdminDashboardDto(
    decimal VentasTotalPeriodo,
    int CantidadVentasPeriodo,
    List<CitasPorDoctorItem> CitasPorDoctor,
    List<ProductoStockBajoItem> ProductosStockBajo);
