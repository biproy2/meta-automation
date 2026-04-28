using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        b.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
        b.Property(x => x.CustomerName).IsRequired().HasMaxLength(150);
        b.Property(x => x.CustomerPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.DeliveryAddress).IsRequired().HasMaxLength(500);
        b.Property(x => x.City).IsRequired().HasMaxLength(100);
        b.Property(x => x.ProductName).IsRequired().HasMaxLength(250);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryCharge).HasPrecision(18, 2);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Status);
        b.HasQueryFilter(x => !x.IsDeleted);
        b.HasOne(x => x.Delivery).WithOne(x => x.Order).HasForeignKey<Delivery>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
