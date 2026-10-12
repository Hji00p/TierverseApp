using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Tests.Common;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateTier> TemplateTiers => Set<TemplateTier>();
    public DbSet<TemplateItem> TemplateItems => Set<TemplateItem>();
    public DbSet<TierListEntity> TierLists => Set<TierListEntity>();
    public DbSet<Tier> Tiers => Set<Tier>();
    public DbSet<TierItem> TierItems => Set<TierItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TierListVote> TierListVotes => Set<TierListVote>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TierListVote>().HasKey(v => new { v.UserId, v.TierListId });
        modelBuilder.Entity<CommentVote>().HasKey(v => new { v.UserId, v.CommentId });
    }

    public static TestApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }
}
