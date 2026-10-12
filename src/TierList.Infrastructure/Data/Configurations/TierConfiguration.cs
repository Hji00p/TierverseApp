using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class TierConfiguration : IEntityTypeConfiguration<Tier>
{
    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.ToTable("Tiers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Order)
            .IsRequired();

        builder.HasMany(t => t.Items)
            .WithOne(item => item.Tier)
            .HasForeignKey(item => item.TierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
