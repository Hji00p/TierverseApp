using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;
using TierList.Infrastructure.Identity;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public new DbSet<User> Users => Set<User>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateTier> TemplateTiers => Set<TemplateTier>();
    public DbSet<TemplateItem> TemplateItems => Set<TemplateItem>();
    public DbSet<TierListEntity> TierLists => Set<TierListEntity>();
    public DbSet<Tier> Tiers => Set<Tier>();
    public DbSet<TierItem> TierItems => Set<TierItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TierListVote> TierListVotes => Set<TierListVote>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
