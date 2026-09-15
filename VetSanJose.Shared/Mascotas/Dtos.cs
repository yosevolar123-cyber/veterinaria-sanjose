namespace VetSanJose.Shared.Mascotas;

public record MascotaDto(
    long Id,
    long ClienteId,
    string Nombre,
    string Especie,
    string? Raza,
    DateOnly? FechaNacimiento,
    string? Sexo,
    decimal? Peso,
    bool Activo);

public record CrearMascotaRequest(
    long? ClienteId,
    string Nombre,
    string Especie,
    string? Raza,
    DateOnly? FechaNacimiento,
    string? Sexo,
    decimal? Peso);

public record ActualizarMascotaRequest(
    string Nombre,
    string Especie,
    string? Raza,
    DateOnly? FechaNacimiento,
    string? Sexo,
    decimal? Peso,
    bool Activo);
