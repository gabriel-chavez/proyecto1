using System.Collections.Generic;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Proporciona acceso a la información y claims del usuario autenticado en la sesión actual.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    string? IPAddress { get; }
    string? UserAgent { get; }
}
