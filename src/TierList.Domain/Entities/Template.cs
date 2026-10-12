using TierList.Domain.Common;
using TierList.Domain.Enums;

namespace TierList.Domain.Entities;

public class Template : BaseEntity
{
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TemplateVisibility Visibility { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<TemplateTier> Tiers { get; private set; } = new List<TemplateTier>();
    public ICollection<TemplateItem> Items { get; private set; } = new List<TemplateItem>();

    private Template() { }

    public Template(Guid authorId, string title, string? description, TemplateVisibility visibility)
    {
        AuthorId = authorId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Visibility = visibility;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string? description, TemplateVisibility visibility)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Visibility = visibility;
        UpdatedAt = DateTime.UtcNow;
    }

    public TemplateTier AddTier(string name, string color, int order)
    {
        var tier = new TemplateTier(Id, name, color, order);
        Tiers.Add(tier);
        UpdatedAt = DateTime.UtcNow;
        return tier;
    }

    public TemplateItem AddItem(string imageUrl, string? caption, int order)
    {
        var item = new TemplateItem(Id, imageUrl, caption, order);
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void ReplaceTiers(IEnumerable<TemplateTier> tiers)
    {
        Tiers.Clear();
        foreach (var tier in tiers)
        {
            Tiers.Add(tier);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceItems(IEnumerable<TemplateItem> items)
    {
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
