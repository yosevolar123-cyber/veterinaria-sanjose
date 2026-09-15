using System.Net.Http.Json;
using VetSanJose.Shared.HistorialesMedicos;

namespace VetSanJose.Client.Services.Api;

public class HistorialMedicoApiClient(HttpClient http)
{
    public async Task<List<HistorialMedicoDto>> GetPorMascotaAsync(long mascotaId)
    {
        return await http.GetFromJsonAsync<List<HistorialMedicoDto>>($"api/historial-medico/mascota/{mascotaId}") ?? [];
    }

    public async Task<HistorialMedicoDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<HistorialMedicoDto>($"api/historial-medico/{id}");
    }

    public async Task<HttpResponseMessage> CrearAsync(CrearHistorialRequest request)
    {
        return await http.PostAsJsonAsync("api/historial-medico", request);
    }
}
