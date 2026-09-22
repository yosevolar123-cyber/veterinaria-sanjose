using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VetSanJose.Application.Abstractions;
using VetSanJose.Application.Common;

namespace VetSanJose.Infrastructure.Storage;

public class SupabaseStorageService(
    HttpClient httpClient,
    IOptions<SupabaseStorageSettings> options,
    ILogger<SupabaseStorageService> logger) : IStorageService
{
    private readonly SupabaseStorageSettings _settings = options.Value;

    public async Task<string> SubirArchivoAsync(
        string carpeta,
        string nombreArchivo,
        string contentType,
        Stream contenido,
        CancellationToken cancellationToken)
    {
        VerificarConfiguracion();

        var rutaObjeto = $"{carpeta}/{nombreArchivo}";

        // Storage de Supabase distingue POST (crear objeto nuevo) de PUT (reemplazar uno existente):
        // un PUT sobre una ruta inexistente responde 404 "Object not found". Como cada subida usa un
        // nombre nuevo, el verbo correcto es POST; x-upsert cubre el caso de una ruta repetida.
        using var request = new HttpRequestMessage(HttpMethod.Post, $"storage/v1/object/{_settings.Bucket}/{rutaObjeto}");
        using var streamContent = new StreamContent(contenido);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = streamContent;
        request.Headers.Add("x-upsert", "true");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Supabase Storage rechazó la subida de {Ruta} en el bucket {Bucket}: {StatusCode} - {Detalle}",
                rutaObjeto, _settings.Bucket, response.StatusCode, detalle);

            throw new AppException(
                "No se pudo guardar la imagen en el almacenamiento. Intente nuevamente más tarde.",
                statusCode: 502);
        }

        return $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{_settings.Bucket}/{rutaObjeto}";
    }

    private void VerificarConfiguracion()
    {
        var faltantes = new List<string>();
        if (string.IsNullOrWhiteSpace(_settings.Url)) faltantes.Add("Supabase__Url");
        if (string.IsNullOrWhiteSpace(_settings.ServiceKey)) faltantes.Add("Supabase__ServiceKey");

        if (faltantes.Count > 0 || httpClient.BaseAddress is null)
        {
            var detalle = faltantes.Count > 0
                ? $"Faltan las variables de entorno: {string.Join(", ", faltantes)}."
                : $"La URL configurada no es absoluta: '{_settings.Url}'.";

            logger.LogError("Storage de Supabase mal configurado. {Detalle}", detalle);

            throw new AppException(
                $"El almacenamiento de imágenes no está configurado en el servidor. {detalle}",
                statusCode: 503);
        }
    }
}
