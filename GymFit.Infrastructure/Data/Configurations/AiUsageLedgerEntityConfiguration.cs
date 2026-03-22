using GymFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class AiUsageLedgerEntityConfiguration : IEntityTypeConfiguration<AiUsageLedger>
{
    public void Configure(EntityTypeBuilder<AiUsageLedger> builder)
    {
        builder.ToTable("ai_usage_ledgers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PeriodKey).HasMaxLength(7).IsRequired();
        builder.Property(x => x.RequestCount).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.PeriodKey }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.AiUsageLedgers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
