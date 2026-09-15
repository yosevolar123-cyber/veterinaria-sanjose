namespace VetSanJose.Shared.Proveedores;

public record ProveedorDto(
    long Id,
    string Nombre,
    string? ContactoNombre,
    string? Telefono,
    string? Email,
    string? Direccion,
    bool Activo);

public record CrearProveedorRequest(
    string Nombre,
    string? ContactoNombre,
    string? Telefono,
    string? Email,
    string? Direccion);

public record ActualizarProveedorRequest(
    string Nombre,
    string? ContactoNombre,
    string? Telefono,
    string? Email,
    string? Direccion,
    bool Activo);
