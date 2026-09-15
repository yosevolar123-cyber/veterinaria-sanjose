using System.Net.Http.Json;
using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Client.Services.Api;

public class UsuariosApiClient(HttpClient http)
{
    public async Task<UsuarioAdminDto?> GetMeAsync()
    {
        return await http.GetFromJsonAsync<UsuarioAdminDto>("api/usuarios/me");
    }

    public async Task<List<UsuarioAdminDto>> GetAllAsync(string? rol = null)
    {
        var url = string.IsNullOrWhiteSpace(rol) ? "api/usuarios" : $"api/usuarios?rol={rol}";
        return await http.GetFromJsonAsync<List<UsuarioAdminDto>>(url) ?? [];
    }

    public async Task<UsuarioAdminDto?> GetByIdAsync(long id)
    {
        return await http.GetFromJsonAsync<UsuarioAdminDto>($"api/usuarios/{id}");
    }

    public async Task<HttpResponseMessage> CrearAsync(CrearUsuarioRequest request)
    {
        return await http.PostAsJsonAsync("api/usuarios", request);
    }

    public async Task<HttpResponseMessage> ActualizarAsync(long id, ActualizarUsuarioRequest request)
    {
        return await http.PutAsJsonAsync($"api/usuarios/{id}", request);
    }
}
