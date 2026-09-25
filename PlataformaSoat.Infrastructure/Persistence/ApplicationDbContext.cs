using Microsoft.EntityFrameworkCore;

namespace PlataformaSoat.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Define DbSets here as needed, e.g.:
    // public DbSet<YourEntity> YourEntities { get; set; }
}
