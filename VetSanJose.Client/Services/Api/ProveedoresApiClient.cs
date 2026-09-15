using System.Net.Http.Json;
using VetSanJose.Shared.Proveedores;

namespace VetSanJose.Client.Services.Api;

public class ProveedoresApiClient(HttpClient http)
{
    public async Task<List<ProveedorDto>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<ProveedorDto>>("api/proveedores") ?? [];
    }

    public async Task<ProveedorDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<ProveedorDto>($"api/proveedores/{id}");
    }

    public async Task<HttpResponseMessage> CrearAsync(CrearProveedorRequest request)
    {
        return await http.PostAsJsonAsync("api/proveedores", request);
    }

    public async Task<HttpResponseMessage> ActualizarAsync(long id, ActualizarProveedorRequest request)
    {
        return await http.PutAsJsonAsync($"api/proveedores/{id}", request);
    }
}
