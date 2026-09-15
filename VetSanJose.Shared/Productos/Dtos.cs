namespace VetSanJose.Shared.Productos;

public record ProductoTiendaDto(
    long Id,
    string Nombre,
    string? Descripcion,
    long? CategoriaId,
    string? CategoriaNombre,
    decimal Precio,
    string? ImagenUrl);

public record ProductoDto(
    long Id,
    string Nombre,
    string? Descripcion,
    long? CategoriaId,
    string? CategoriaNombre,
    decimal Precio,
    int Stock,
    string? ImagenUrl,
    long? ProveedorId,
    string? ProveedorNombre,
    bool Activo,
    string Tipo = "venta_publico");

public record CrearProductoRequest(
    string Nombre,
    string? Descripcion,
    long? CategoriaId,
    decimal Precio,
    int Stock,
    string? ImagenUrl,
    long? ProveedorId,
    string Tipo = "venta_publico");

public record ActualizarProductoRequest(
    string Nombre,
    string? Descripcion,
    long? CategoriaId,
    decimal Precio,
    string? ImagenUrl,
    long? ProveedorId,
    bool Activo,
    string Tipo = "venta_publico");

public record AjustarStockRequest(int Cantidad);
