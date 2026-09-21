using System.Net.Http.Json;

namespace VetSanJose.Client.Services.Api;

public class ArchivosApiClient(HttpClient http)
{
    public async Task<(bool Exito, string? Url, string? Error)> SubirImagenAsync(string carpeta, Stream contenido, string nombreArchivo, string contentType)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(contenido);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(new StringContent(carpeta), "carpeta");
        form.Add(fileContent, "archivo", nombreArchivo);

        var response = await http.PostAsync("api/archivos/imagenes", form);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.LeerMensajeAsync();
            return (false, null, error);
        }

        var resultado = await response.Content.ReadFromJsonAsync<SubirImagenResponse>();
        return (true, resultado?.Url, null);
    }

    private record SubirImagenResponse(string Url);
}
