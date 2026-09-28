using System;
using PlataformaSoat.Domain.Core;

namespace PlataformaSoat.Domain.Entities;

/// <summary>
/// Entidad de usuario persistida en la tabla security.tb_user de PostgreSQL.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
