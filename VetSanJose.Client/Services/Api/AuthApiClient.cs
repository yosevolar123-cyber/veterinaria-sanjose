using System.Net.Http.Json;
using VetSanJose.Shared.Auth;

namespace VetSanJose.Client.Services.Api;

public class AuthApiClient(HttpClient http)
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RegistrarClienteAsync(RegistroClienteRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/registro-cliente", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var response = await http.PostAsJsonAsync("api/auth/refresh", new RefreshRequest(refreshToken));
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }
}
