using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class TrainerOrderEntityConfiguration : IEntityTypeConfiguration<TrainerOrder>
{
    public void Configure(EntityTypeBuilder<TrainerOrder> builder)
    {
        builder.ToTable("trainer_orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(12, 2).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<TrainerOrderStatus>())
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TrainerId);
        builder.HasIndex(x => x.TrainerProfileId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}
