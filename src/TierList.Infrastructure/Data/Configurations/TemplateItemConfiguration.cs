using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class TemplateItemConfiguration : IEntityTypeConfiguration<TemplateItem>
{
    public void Configure(EntityTypeBuilder<TemplateItem> builder)
    {
        builder.ToTable("TemplateItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Caption)
            .HasMaxLength(100);

        builder.Property(t => t.Order)
            .IsRequired();

        builder.HasIndex(t => t.TemplateId);
    }
}
