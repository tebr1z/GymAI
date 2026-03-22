using GymFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymFit.Infrastructure.Data.Configurations;

public sealed class MessageEntityConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageText).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.SenderId);
        builder.HasIndex(x => x.ReceiverId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.ReceiverId, x.CreatedAt });
        builder.HasIndex(x => new { x.SenderId, x.ReceiverId, x.CreatedAt });
    }
}
