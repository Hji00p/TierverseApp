using Microsoft.EntityFrameworkCore;
using TierList.Domain.Entities;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Template> Templates { get; }
    DbSet<TemplateTier> TemplateTiers { get; }
    DbSet<TemplateItem> TemplateItems { get; }
    DbSet<TierListEntity> TierLists { get; }
    DbSet<Tier> Tiers { get; }
    DbSet<TierItem> TierItems { get; }
    DbSet<Comment> Comments { get; }
    DbSet<TierListVote> TierListVotes { get; }
    DbSet<CommentVote> CommentVotes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
