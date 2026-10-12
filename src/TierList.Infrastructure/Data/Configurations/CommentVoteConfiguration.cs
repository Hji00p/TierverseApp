using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierList.Domain.Entities;

namespace TierList.Infrastructure.Data.Configurations;

public class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
{
    public void Configure(EntityTypeBuilder<CommentVote> builder)
    {
        builder.ToTable("CommentVotes");

        builder.HasKey(v => new { v.UserId, v.CommentId });

        builder.Property(v => v.Value)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.HasOne(v => v.User)
            .WithMany(u => u.CommentVotes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Comment)
            .WithMany(c => c.Votes)
            .HasForeignKey(v => v.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
