using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class TierListVoteConfiguration : IEntityTypeConfiguration<TierListVote>
{
    public void Configure(EntityTypeBuilder<TierListVote> builder)
    {
        builder.ToTable("TierListVotes");

        builder.HasKey(v => new { v.UserId, v.TierListId });

        builder.Property(v => v.Value)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.HasOne(v => v.User)
            .WithMany(u => u.TierListVotes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.TierList)
            .WithMany(t => t.Votes)
            .HasForeignKey(v => v.TierListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
