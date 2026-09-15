namespace VetSanJose.Shared.Usuarios;

public record UsuarioAdminDto(
    long Id,
    string Nombre,
    string Apellido,
    string Email,
    string Rol,
    string? Telefono,
    bool Activo,
    string? Especialidad = null,
    string? Matricula = null);

public record CrearUsuarioRequest(
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    string Rol,
    string? Telefono,
    string? Especialidad = null,
    string? Matricula = null);

public record ActualizarUsuarioRequest(
    string Nombre,
    string Apellido,
    string Rol,
    string? Telefono,
    bool Activo,
    string? Especialidad = null,
    string? Matricula = null);
