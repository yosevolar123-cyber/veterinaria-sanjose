using System.Net.Http.Json;
using VetSanJose.Shared.Ventas;

namespace VetSanJose.Client.Services.Api;

public class VentasApiClient(HttpClient http)
{
    public async Task<HttpResponseMessage> CrearAsync(CrearVentaRequest request)
    {
        return await http.PostAsJsonAsync("api/ventas", request);
    }

    public async Task<List<VentaDto>> GetMisVentasAsync()
    {
        return await http.GetFromJsonAsync<List<VentaDto>>("api/ventas/mias") ?? [];
    }

    public async Task<List<VentaDto>> GetPorClienteAsync(long clienteId)
    {
        return await http.GetFromJsonAsync<List<VentaDto>>($"api/ventas/cliente/{clienteId}") ?? [];
    }
}
