using System.Net.Http.Json;
using VetSanJose.Shared.Reportes;

namespace VetSanJose.Client.Services.Api;

public class ReportesApiClient(HttpClient http)
{
    public async Task<ReporteFinancieroDto?> GetFinancieroAsync(DateOnly? desde = null, DateOnly? hasta = null)
    {
        return await http.GetFromJsonAsync<ReporteFinancieroDto>("api/reportes/financiero" + ConstruirQuery(desde, hasta));
    }

    public async Task<(bool Exito, byte[]? Pdf, string? Error)> DescargarFinancieroPdfAsync(DateOnly? desde = null, DateOnly? hasta = null)
    {
        var response = await http.GetAsync("api/reportes/financiero/pdf" + ConstruirQuery(desde, hasta));
        if (!response.IsSuccessStatusCode)
        {
            return (false, null, await response.LeerMensajeAsync());
        }

        return (true, await response.Content.ReadAsByteArrayAsync(), null);
    }

    private static string ConstruirQuery(DateOnly? desde, DateOnly? hasta)
    {
        var query = new List<string>();
        if (desde.HasValue) query.Add($"desde={desde:yyyy-MM-dd}");
        if (hasta.HasValue) query.Add($"hasta={hasta:yyyy-MM-dd}");
        return query.Count > 0 ? "?" + string.Join("&", query) : "";
    }
}
