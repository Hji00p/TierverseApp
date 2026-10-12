using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class Tier : BaseEntity
{
    public Guid TierListId { get; private set; }
    public TierList TierList { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public int Order { get; private set; }

    public ICollection<TierItem> Items { get; private set; } = new List<TierItem>();

    private Tier() { }

    public Tier(Guid tierListId, string name, string color, int order)
    {
        TierListId = tierListId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Order = order;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string color, int order)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Order = order;
    }
}
