using VetSanJose.Shared.Dashboard;

namespace VetSanJose.Application.Dashboard;

public interface IDashboardService
{
    Task<DoctorDashboardDto> GetDoctorAsync(CancellationToken cancellationToken);
    Task<SecretariaDashboardDto> GetSecretariaAsync(CancellationToken cancellationToken);
    Task<AdminDashboardDto> GetAdminAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken);
}
