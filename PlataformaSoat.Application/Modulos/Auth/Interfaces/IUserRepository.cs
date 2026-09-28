using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Modulos.Auth.DTOs;

namespace PlataformaSoat.Application.Modulos.Auth.Interfaces;

/// <summary>
/// Contrato de persistencia para consultar usuarios del sistema desde la base de datos PostgreSQL.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene las credenciales y datos del usuario por nombre de usuario.
    /// </summary>
    Task<(string PasswordHash, UserInfoDto UserInfo)?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
