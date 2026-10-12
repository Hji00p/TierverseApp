using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Infrastructure.Data.Configurations;

public class TierListConfiguration : IEntityTypeConfiguration<TierListEntity>
{
    public void Configure(EntityTypeBuilder<TierListEntity> builder)
    {
        builder.ToTable("TierLists");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasIndex(t => t.Title);
        builder.HasIndex(t => t.TemplateId);

        builder.HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Template)
            .WithMany()
            .HasForeignKey(t => t.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Tiers)
            .WithOne(tier => tier.TierList)
            .HasForeignKey(tier => tier.TierListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Items)
            .WithOne(item => item.TierList)
            .HasForeignKey(item => item.TierListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.TierList)
            .HasForeignKey(c => c.TierListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Votes)
            .WithOne(v => v.TierList)
            .HasForeignKey(v => v.TierListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
