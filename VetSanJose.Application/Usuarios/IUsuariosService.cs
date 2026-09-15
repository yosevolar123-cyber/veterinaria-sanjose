using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

public interface IUsuariosService
{
    Task<UsuarioAdminDto> GetMeAsync(CancellationToken cancellationToken);
    Task<List<UsuarioAdminDto>> GetAllAsync(string? rol, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> CrearAsync(CrearUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> ActualizarAsync(long id, ActualizarUsuarioRequest request, CancellationToken cancellationToken);
}
