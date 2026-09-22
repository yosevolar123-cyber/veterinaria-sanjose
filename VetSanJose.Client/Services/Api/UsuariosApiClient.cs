using System.Net.Http.Json;
using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Client.Services.Api;

public class UsuariosApiClient(HttpClient http)
{
    public async Task<UsuarioAdminDto?> GetMeAsync()
    {
        return await http.GetFromJsonAsync<UsuarioAdminDto>("api/usuarios/me");
    }

    public async Task<List<UsuarioAdminDto>> GetAllAsync(string? rol = null, bool soloActivos = false)
    {
        var filtros = new List<string>();
        if (!string.IsNullOrWhiteSpace(rol)) filtros.Add($"rol={rol}");
        if (soloActivos) filtros.Add("soloActivos=true");

        var url = filtros.Count > 0 ? $"api/usuarios?{string.Join("&", filtros)}" : "api/usuarios";
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

    public async Task<HttpResponseMessage> CambiarEstadoAsync(long id, bool activo)
    {
        return await http.PatchAsJsonAsync($"api/usuarios/{id}/estado", new CambiarEstadoUsuarioRequest(activo));
    }

    public async Task<HttpResponseMessage> CambiarRolAsync(long id, string rol)
    {
        return await http.PatchAsJsonAsync($"api/usuarios/{id}/rol", new CambiarRolUsuarioRequest(rol));
    }
}
