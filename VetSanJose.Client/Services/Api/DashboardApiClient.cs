using System.Net.Http.Json;
using VetSanJose.Shared.Dashboard;

namespace VetSanJose.Client.Services.Api;

public class DashboardApiClient(HttpClient http)
{
    public async Task<DoctorDashboardDto?> GetDoctorAsync()
    {
        return await http.GetFromJsonAsync<DoctorDashboardDto>("api/dashboard/doctor");
    }

    public async Task<SecretariaDashboardDto?> GetSecretariaAsync()
    {
        return await http.GetFromJsonAsync<SecretariaDashboardDto>("api/dashboard/secretaria");
    }

    public async Task<AdminDashboardDto?> GetAdminAsync(DateOnly? desde = null, DateOnly? hasta = null)
    {
        var query = new List<string>();
        if (desde.HasValue) query.Add($"desde={desde:yyyy-MM-dd}");
        if (hasta.HasValue) query.Add($"hasta={hasta:yyyy-MM-dd}");
        var url = "api/dashboard/admin" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await http.GetFromJsonAsync<AdminDashboardDto>(url);
    }
}
