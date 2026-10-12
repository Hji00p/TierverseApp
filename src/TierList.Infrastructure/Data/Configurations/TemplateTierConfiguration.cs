using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class TemplateTierConfiguration : IEntityTypeConfiguration<TemplateTier>
{
    public void Configure(EntityTypeBuilder<TemplateTier> builder)
    {
        builder.ToTable("TemplateTiers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Color)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(t => t.Order)
            .IsRequired();

        builder.HasIndex(t => t.TemplateId);
    }
}
