using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> b)
    {
        b.ToTable("Deliveries");
        b.HasKey(x => x.Id);
        b.Property(x => x.PathaoConsignmentId).HasMaxLength(100);
        b.Property(x => x.TrackingCode).HasMaxLength(100);
        b.Property(x => x.RecipientName).IsRequired().HasMaxLength(150);
        b.Property(x => x.RecipientPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.RecipientAddress).IsRequired().HasMaxLength(500);
        b.Property(x => x.RecipientCity).IsRequired().HasMaxLength(100);
        b.Property(x => x.CollectAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryFee).HasPrecision(18, 2);
        b.Property(x => x.Weight).HasPrecision(10, 3);
        b.HasIndex(x => x.PathaoConsignmentId);
        b.HasIndex(x => x.TrackingCode);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
