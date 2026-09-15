using System.Net.Http.Json;
using VetSanJose.Shared.Productos;

namespace VetSanJose.Client.Services.Api;

public class ProductosApiClient(HttpClient http)
{
    public async Task<List<ProductoTiendaDto>> GetTiendaAsync(long? categoriaId = null)
    {
        var url = categoriaId.HasValue ? $"api/productos/tienda?categoriaId={categoriaId}" : "api/productos/tienda";
        return await http.GetFromJsonAsync<List<ProductoTiendaDto>>(url) ?? [];
    }

    public async Task<List<ProductoDto>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? [];
    }

    public async Task<List<ProductoDto>> GetStockBajoAsync(int umbral = 5)
    {
        return await http.GetFromJsonAsync<List<ProductoDto>>($"api/productos/stock-bajo?umbral={umbral}") ?? [];
    }

    public async Task<ProductoDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<ProductoDto>($"api/productos/{id}");
    }

    public async Task<HttpResponseMessage> CrearAsync(CrearProductoRequest request)
    {
        return await http.PostAsJsonAsync("api/productos", request);
    }

    public async Task<HttpResponseMessage> ActualizarAsync(long id, ActualizarProductoRequest request)
    {
        return await http.PutAsJsonAsync($"api/productos/{id}", request);
    }

    public async Task<HttpResponseMessage> AjustarStockAsync(long id, AjustarStockRequest request)
    {
        return await http.PatchAsJsonAsync($"api/productos/{id}/stock", request);
    }
}
