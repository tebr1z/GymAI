using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class PlanEntityConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasConversion(new EnumToStringConverter<FitnessPlanType>())
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.TrainerId);
        builder.HasIndex(x => new { x.UserId, x.Type });

        // Paginated "my plans": WHERE UserId ORDER BY CreatedAt DESC.
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
