using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Ecommerce.Persistence.DbContext;

/// <summary>
/// Main EF Core entry point.
/// DbSet = each one is a queryable table.
/// ApplyConfigurationsFromAssembly = loads Fluent API config from the Configurations/ folder.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User>       Users       => Set<User>();
    public DbSet<Lead>       Leads       => Set<Lead>();
    public DbSet<Order>      Orders      => Set<Order>();
    public DbSet<Delivery>   Deliveries  => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Scans this assembly and applies every IEntityTypeConfiguration<T> class automatically
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set UpdatedAt on every modified entity
        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                     .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
