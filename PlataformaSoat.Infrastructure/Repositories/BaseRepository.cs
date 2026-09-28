using Microsoft.Extensions.Logging;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly ILogger<BaseRepository> _logger;

    protected BaseRepository(ApplicationDbContext dbContext, ILogger<BaseRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Resuelve el usuario de registro priorizando el usuario autenticado en la sesión actual.
    /// </summary>
    protected string ResolveRegisteredBy(string? fallback = null)
    {
        if (!string.IsNullOrWhiteSpace(_dbContext.CurrentUserService?.Username))
        {
            return _dbContext.CurrentUserService.Username;
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback;
        }

        return "sistema";
    }
}
