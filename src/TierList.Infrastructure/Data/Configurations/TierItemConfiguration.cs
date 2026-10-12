using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class TierItemConfiguration : IEntityTypeConfiguration<TierItem>
{
    public void Configure(EntityTypeBuilder<TierItem> builder)
    {
        builder.ToTable("TierItems");

        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.ImageUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(ti => ti.Caption)
            .HasMaxLength(200);

        builder.Property(ti => ti.Position)
            .IsRequired();

        builder.Property(ti => ti.TierId)
            .IsRequired(false);
    }
}
