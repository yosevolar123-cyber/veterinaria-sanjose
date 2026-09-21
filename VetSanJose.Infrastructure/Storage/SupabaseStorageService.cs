using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using VetSanJose.Application.Abstractions;

namespace VetSanJose.Infrastructure.Storage;

public class SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseStorageSettings> options) : IStorageService
{
    private readonly SupabaseStorageSettings _settings = options.Value;

    public async Task<string> SubirArchivoAsync(
        string carpeta,
        string nombreArchivo,
        string contentType,
        Stream contenido,
        CancellationToken cancellationToken)
    {
        var rutaObjeto = $"{carpeta}/{nombreArchivo}";
        using var request = new HttpRequestMessage(HttpMethod.Put, $"storage/v1/object/{_settings.Bucket}/{rutaObjeto}");
        using var streamContent = new StreamContent(contenido);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = streamContent;
        request.Headers.Add("x-upsert", "true");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"No se pudo subir el archivo a Supabase Storage: {response.StatusCode} - {detalle}");
        }

        return $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{_settings.Bucket}/{rutaObjeto}";
    }
}
