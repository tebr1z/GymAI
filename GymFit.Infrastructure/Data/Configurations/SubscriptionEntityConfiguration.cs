using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class SubscriptionEntityConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tier)
            .HasConversion(new EnumToStringConverter<SubscriptionTier>())
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<SubscriptionStatus>())
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();

        builder.Property(x => x.ExternalProvider).HasMaxLength(64);
        builder.Property(x => x.ExternalCustomerId).HasMaxLength(256);
        builder.Property(x => x.ExternalSubscriptionId).HasMaxLength(256);

        builder.HasIndex(x => x.EndDate);
        builder.HasIndex(x => new { x.ExternalProvider, x.ExternalSubscriptionId });

        // Active subscription: WHERE UserId AND Status AND date range (OrderBy EndDate in queries).
        builder.HasIndex(x => new { x.UserId, x.Status, x.EndDate });
        builder.HasIndex(x => new { x.UserId, x.Status, x.StartDate });
    }
}
