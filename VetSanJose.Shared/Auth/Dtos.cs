namespace VetSanJose.Shared.Auth;

public record LoginRequest(string Email, string Password);

public record RegistroClienteRequest(
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    string? Telefono);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UsuarioDto Usuario);

public record UsuarioDto(
    long Id,
    string Nombre,
    string Apellido,
    string Email,
    string Rol,
    string? Telefono,
    bool Activo);
