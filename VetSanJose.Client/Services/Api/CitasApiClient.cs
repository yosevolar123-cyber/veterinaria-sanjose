using System.Net.Http.Json;
using VetSanJose.Shared.Citas;

namespace VetSanJose.Client.Services.Api;

public class CitasApiClient(HttpClient http)
{
    public async Task<List<CitaDto>> GetMisCitasAsync()
    {
        return await http.GetFromJsonAsync<List<CitaDto>>("api/citas/mias") ?? [];
    }

    public async Task<List<CitaDto>> GetAgendaAsync(long? doctorId = null, DateOnly? fecha = null)
    {
        var query = new List<string>();
        if (doctorId.HasValue) query.Add($"doctorId={doctorId}");
        if (fecha.HasValue) query.Add($"fecha={fecha:yyyy-MM-dd}");
        var url = "api/citas/agenda" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await http.GetFromJsonAsync<List<CitaDto>>(url) ?? [];
    }

    public async Task<CitaDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<CitaDto>($"api/citas/{id}");
    }

    public async Task<CitaDto?> CrearAsync(CrearCitaRequest request)
    {
        var response = await http.PostAsJsonAsync("api/citas", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CitaDto>();
    }

    public async Task<CitaDto?> ActualizarAsync(long id, ActualizarCitaRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/citas/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CitaDto>();
    }
}
