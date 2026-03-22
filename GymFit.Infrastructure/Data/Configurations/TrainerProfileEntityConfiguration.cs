using GymFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class TrainerProfileEntityConfiguration : IEntityTypeConfiguration<TrainerProfile>
{
    public void Configure(EntityTypeBuilder<TrainerProfile> builder)
    {
        builder.ToTable("trainer_profiles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.Bio).HasMaxLength(4000);
        builder.Property(x => x.ExperienceYears).IsRequired();
        builder.Property(x => x.PricePerMonth).HasPrecision(12, 2).IsRequired();
        builder.Property(x => x.Rating).HasPrecision(4, 2).IsRequired();
        builder.Property(x => x.IsApproved).IsRequired();

        // Marketplace: WHERE IsApproved AND optional rating/price filters, ORDER BY Rating.
        builder.HasIndex(x => new { x.IsApproved, x.Rating });
        builder.HasIndex(x => new { x.IsApproved, x.PricePerMonth });

        builder.HasMany(x => x.TrainerOrders)
            .WithOne(x => x.TrainerProfile)
            .HasForeignKey(x => x.TrainerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
