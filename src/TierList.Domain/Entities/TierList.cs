using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class TierList : BaseEntity
{
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;
    public Guid? TemplateId { get; private set; }
    public Template? Template { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Tier> Tiers { get; private set; } = new List<Tier>();
    public ICollection<TierItem> Items { get; private set; } = new List<TierItem>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<TierListVote> Votes { get; private set; } = new List<TierListVote>();

    private TierList() { }

    public TierList(Guid authorId, string title, string? description = null, Guid? templateId = null)
    {
        AuthorId = authorId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        TemplateId = templateId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public Tier AddTier(string name, string color, int order)
    {
        var tier = new Tier(Id, name, color, order);
        Tiers.Add(tier);
        UpdatedAt = DateTime.UtcNow;
        return tier;
    }

    public TierItem AddItem(string imageUrl, string? caption, Guid? tierId = null, int position = 0)
    {
        var item = new TierItem(Id, imageUrl, caption, tierId, position);
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
