namespace VetSanJose.Shared.Ventas;

public record DetalleVentaDto(long ProductoId, string ProductoNombre, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record VentaDto(long Id, long ClienteId, DateTimeOffset Fecha, decimal Total, string Estado, List<DetalleVentaDto> Detalles);

public record ItemCarritoRequest(long ProductoId, int Cantidad);

public record CrearVentaRequest(List<ItemCarritoRequest> Items);
