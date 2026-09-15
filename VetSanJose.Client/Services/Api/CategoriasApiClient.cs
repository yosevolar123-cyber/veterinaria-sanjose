using System.Net.Http.Json;
using VetSanJose.Shared.Categorias;

namespace VetSanJose.Client.Services.Api;

public class CategoriasApiClient(HttpClient http)
{
    public async Task<List<CategoriaProductoDto>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<CategoriaProductoDto>>("api/categorias-producto") ?? [];
    }

    public async Task<HttpResponseMessage> CrearAsync(CrearCategoriaRequest request)
    {
        return await http.PostAsJsonAsync("api/categorias-producto", request);
    }

    public async Task<HttpResponseMessage> ActualizarAsync(long id, ActualizarCategoriaRequest request)
    {
        return await http.PutAsJsonAsync($"api/categorias-producto/{id}", request);
    }
}
