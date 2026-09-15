namespace VetSanJose.Shared.Categorias;

public record CategoriaProductoDto(long Id, string Nombre, string? Descripcion);

public record CrearCategoriaRequest(string Nombre, string? Descripcion);

public record ActualizarCategoriaRequest(string Nombre, string? Descripcion);
