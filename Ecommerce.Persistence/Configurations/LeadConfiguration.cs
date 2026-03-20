using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("Leads");
        b.HasKey(x => x.Id);

        b.Property(x => x.CustomerName).IsRequired().HasMaxLength(150);
        b.Property(x => x.CustomerPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.CustomerEmail).HasMaxLength(150);
        b.Property(x => x.ProductInterest).IsRequired().HasMaxLength(500);
        b.Property(x => x.IncomingMessage).HasMaxLength(2000);
        b.Property(x => x.ChannelUserId).HasMaxLength(200);
        b.Property(x => x.Notes).HasMaxLength(1000);

        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.Source).IsRequired();

        b.HasQueryFilter(x => !x.IsDeleted);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CustomerPhone);

        b.HasOne(x => x.User)
         .WithMany(x => x.Leads)
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
