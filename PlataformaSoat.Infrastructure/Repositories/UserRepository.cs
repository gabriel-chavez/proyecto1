using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Modulos.Auth.DTOs;
using PlataformaSoat.Application.Modulos.Auth.Interfaces;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios utilizando Entity Framework Core y la tabla security.tb_user.
/// </summary>
public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext, ILogger<UserRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<(string PasswordHash, UserInfoDto UserInfo)?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando usuario {Username} en la base de datos (security.tb_user)", username);

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);

        if (user == null)
        {
            return null;
        }

        var rolesList = string.IsNullOrWhiteSpace(user.Roles)
            ? new List<string>()
            : user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var userInfo = new UserInfoDto
        {
            UserId = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            Roles = rolesList
        };

        return (user.PasswordHash, userInfo);
    }
}
