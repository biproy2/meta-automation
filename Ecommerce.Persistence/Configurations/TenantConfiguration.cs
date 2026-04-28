using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("Tenants");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.OwnerEmail).IsRequired().HasMaxLength(150);
        b.HasIndex(x => x.OwnerEmail).IsUnique();
        b.Property(x => x.OwnerName).IsRequired().HasMaxLength(150);
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        b.HasQueryFilter(x => !x.IsDeleted);
        b.HasOne(x => x.Settings).WithOne(x => x.Tenant).HasForeignKey<TenantSettings>(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Orders).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Leads).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
