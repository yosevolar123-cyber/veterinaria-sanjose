namespace VetSanJose.Application.Abstractions;

public interface IStorageService
{
    Task<string> SubirArchivoAsync(
        string carpeta,
        string nombreArchivo,
        string contentType,
        Stream contenido,
        CancellationToken cancellationToken);
}
