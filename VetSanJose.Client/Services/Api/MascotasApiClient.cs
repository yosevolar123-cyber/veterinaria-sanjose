using System.Net.Http.Json;
using VetSanJose.Shared.Mascotas;

namespace VetSanJose.Client.Services.Api;

public class MascotasApiClient(HttpClient http)
{
    public async Task<List<MascotaDto>> GetMisMascotasAsync()
    {
        return await http.GetFromJsonAsync<List<MascotaDto>>("api/mascotas/mias") ?? [];
    }

    public async Task<List<MascotaDto>> GetPorClienteAsync(long clienteId)
    {
        return await http.GetFromJsonAsync<List<MascotaDto>>($"api/mascotas/cliente/{clienteId}") ?? [];
    }

    public async Task<MascotaDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<MascotaDto>($"api/mascotas/{id}");
    }

    public async Task<MascotaDto?> CrearAsync(CrearMascotaRequest request)
    {
        var response = await http.PostAsJsonAsync("api/mascotas", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MascotaDto>();
    }

    public async Task<MascotaDto?> ActualizarAsync(long id, ActualizarMascotaRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/mascotas/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MascotaDto>();
    }
}
