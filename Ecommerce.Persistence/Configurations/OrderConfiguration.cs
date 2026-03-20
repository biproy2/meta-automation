using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the Orders table.
/// Keeps entity clean — no [Column] or [MaxLength] data annotations needed.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.OrderNumber).IsUnique();

        b.Property(x => x.CustomerName).IsRequired().HasMaxLength(150);
        b.Property(x => x.CustomerPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.DeliveryAddress).IsRequired().HasMaxLength(500);
        b.Property(x => x.City).IsRequired().HasMaxLength(100);
        b.Property(x => x.ProductName).IsRequired().HasMaxLength(250);
        b.Property(x => x.ProductSku).HasMaxLength(100);

        // Store money as decimal(18,2)
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryCharge).HasPrecision(18, 2);

        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.OrderSource).IsRequired();

        b.Property(x => x.ChannelUserId).HasMaxLength(200);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.InternalNotes).HasMaxLength(1000);

        // Soft delete: exclude IsDeleted=true rows from all queries
        b.HasQueryFilter(x => !x.IsDeleted);

        // Index for common queries
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CustomerPhone);
        b.HasIndex(x => x.CreatedAt);

        // One Order → One Delivery
        b.HasOne(x => x.Delivery)
         .WithOne(x => x.Order)
         .HasForeignKey<Delivery>(x => x.OrderId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
