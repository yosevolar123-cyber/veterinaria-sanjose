using VetSanJose.Shared.Auth;

namespace VetSanJose.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RegistrarClienteAsync(RegistroClienteRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
}
