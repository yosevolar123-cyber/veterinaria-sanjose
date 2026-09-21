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
