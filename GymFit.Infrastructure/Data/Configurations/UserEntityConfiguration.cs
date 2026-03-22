using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();

        builder.Property(x => x.Role)
            .HasConversion(new EnumToStringConverter<UserRole>())
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Weight).HasPrecision(8, 2);
        builder.Property(x => x.Height).HasPrecision(8, 2);
        builder.Property(x => x.Goal).HasMaxLength(2000);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Role);

        builder.HasOne(x => x.TrainerProfile)
            .WithOne(x => x.User)
            .HasForeignKey<TrainerProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Plans)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CoachingPlans)
            .WithOne(x => x.Trainer)
            .HasForeignKey(x => x.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SentMessages)
            .WithOne(x => x.Sender)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReceivedMessages)
            .WithOne(x => x.Receiver)
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ClientOrders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TrainerOrders)
            .WithOne(x => x.Trainer)
            .HasForeignKey(x => x.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
