using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Configuration;
using PlataformaSoat.Domain.Entities;

namespace PlataformaSoat.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ICurrentUserService? CurrentUserService { get; }
    public ApiSettings? ApiSettings { get; }
    public ITransactionIoLogService? TransactionIoLogService { get; }
    public IErrorLogService? ErrorLogService { get; }
    public ILogger<ApplicationDbContext>? Logger { get; }

    public DbSet<User> Users => Set<User>();

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("tb_user", "security");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(e => e.Roles).HasColumnName("roles").HasMaxLength(255);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreadoEn).HasColumnName("created_at");
            entity.Property(e => e.CreadoPor).HasColumnName("created_by");
            entity.Property(e => e.ModificadoEn).HasColumnName("updated_at");
            entity.Property(e => e.ModificadoPor).HasColumnName("updated_by");
        });
    }
}
