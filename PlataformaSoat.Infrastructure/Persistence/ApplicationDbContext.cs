using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Configuration;

namespace PlataformaSoat.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ICurrentUserService? CurrentUserService { get; }
    public ApiSettings? ApiSettings { get; }
    public ITransactionIoLogService? TransactionIoLogService { get; }
    public IErrorLogService? ErrorLogService { get; }
    public ILogger<ApplicationDbContext>? Logger { get; }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IOptions<ApiSettings>? apiSettings = null,
        ITransactionIoLogService? transactionIoLogService = null,
        IErrorLogService? errorLogService = null,
        ILogger<ApplicationDbContext>? logger = null)
        : base(options)
    {
        CurrentUserService = currentUserService;
        ApiSettings = apiSettings?.Value;
        TransactionIoLogService = transactionIoLogService;
        ErrorLogService = errorLogService;
        Logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
