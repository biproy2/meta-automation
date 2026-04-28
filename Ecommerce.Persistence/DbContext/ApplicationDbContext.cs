using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Ecommerce.Persistence.DbContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Modified))
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        return base.SaveChangesAsync(cancellationToken);
    }
}
