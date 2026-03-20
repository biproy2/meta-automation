using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.MessengerPsid).HasMaxLength(100);
        b.Property(x => x.WhatsAppNumber).HasMaxLength(20);
        b.HasIndex(x => x.Phone).IsUnique();
        b.HasQueryFilter(x => !x.IsDeleted);
        b.HasMany(x => x.Orders).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
