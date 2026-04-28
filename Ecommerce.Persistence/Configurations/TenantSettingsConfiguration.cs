using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Persistence.Configurations;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> b)
    {
        b.ToTable("TenantSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.WhatsAppPhoneNumberId).HasMaxLength(100);
        b.Property(x => x.WhatsAppAccessToken).HasMaxLength(1000);
        b.Property(x => x.WhatsAppWebhookToken).HasMaxLength(200);
        b.Property(x => x.MessengerPageToken).HasMaxLength(1000);
        b.Property(x => x.MessengerAppSecret).HasMaxLength(200);
        b.Property(x => x.PathaoClientId).HasMaxLength(100);
        b.Property(x => x.PathaoClientSecret).HasMaxLength(200);
        b.Property(x => x.PathaoUsername).HasMaxLength(150);
        b.Property(x => x.PathaoPassword).HasMaxLength(200);
        b.Property(x => x.PathaoStoreId).HasMaxLength(50);
        b.Property(x => x.ShopifyStoreUrl).HasMaxLength(200);
        b.Property(x => x.ShopifyAccessToken).HasMaxLength(500);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
