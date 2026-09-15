using System.Net.Http.Json;

namespace VetSanJose.Client.Services.Api;

public static class ApiErrorHelper
{
    public static async Task<string> LeerMensajeAsync(this HttpResponseMessage response)
    {
        try
        {
            var problema = await response.Content.ReadFromJsonAsync<ProblemaApi>();
            if (!string.IsNullOrWhiteSpace(problema?.Detail))
            {
                return problema.Detail;
            }

            if (problema?.Errors is { Count: > 0 })
            {
                return string.Join(" ", problema.Errors.SelectMany(e => e.Value));
            }
        }
        catch
        {
            // ignorar y usar mensaje genérico
        }

        return "Ocurrió un error inesperado. Intenta nuevamente.";
    }

    private class ProblemaApi
    {
        public string? Detail { get; set; }
        public Dictionary<string, List<string>>? Errors { get; set; }
    }
}
