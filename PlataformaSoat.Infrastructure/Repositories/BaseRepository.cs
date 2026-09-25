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
}
