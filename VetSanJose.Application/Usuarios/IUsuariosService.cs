using VetSanJose.Shared.Usuarios;

namespace VetSanJose.Application.Usuarios;

public interface IUsuariosService
{
    Task<UsuarioAdminDto> GetMeAsync(CancellationToken cancellationToken);
    Task<List<UsuarioAdminDto>> GetAllAsync(string? rol, bool? soloActivos, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> CrearAsync(CrearUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> ActualizarAsync(long id, ActualizarUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> CambiarEstadoAsync(long id, bool activo, CancellationToken cancellationToken);
    Task<UsuarioAdminDto> CambiarRolAsync(long id, string rol, CancellationToken cancellationToken);
}
