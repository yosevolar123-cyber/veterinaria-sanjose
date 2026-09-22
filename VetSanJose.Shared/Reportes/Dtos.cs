namespace VetSanJose.Shared.Reportes;

public record ReporteFinancieroItemDto(
    DateOnly Fecha,
    decimal Ingresos,
    decimal Costos,
    int ClientesAtendidos,
    int Consultas);

public record ReporteFinancieroDto(
    DateOnly Desde,
    DateOnly Hasta,
    List<ReporteFinancieroItemDto> Items,
    decimal TotalIngresos,
    decimal TotalCostos,
    int TotalClientesAtendidos,
    int TotalConsultas);

public record ProductoVendidoDto(
    long ProductoId,
    string Nombre,
    int CantidadVendida,
    decimal MontoVendido);

public record ReporteNegocioMesDto(
    int Anio,
    int Mes,
    decimal VentasMontoTotal,
    int VentasCantidad,
    ProductoVendidoDto? ProductoMasVendido,
    ProductoVendidoDto? ProductoMenosVendido,
    int ProductosSinVentasEnElMes,
    List<ProductoVendidoDto> TopProductos,
    int StockTotalUnidades,
    int ProductosConStockBajo,
    int UmbralStockBajo,
    decimal ValorTotalInventario);
