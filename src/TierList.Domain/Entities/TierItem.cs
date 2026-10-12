using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class TierItem : BaseEntity
{
    public Guid TierListId { get; private set; }
    public TierList TierList { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public Guid? TierId { get; private set; }
    public Tier? Tier { get; private set; }
    public int Position { get; private set; }

    private TierItem() { }

    public TierItem(Guid tierListId, string imageUrl, string? caption = null, Guid? tierId = null, int position = 0)
    {
        TierListId = tierListId;
        ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
        Caption = caption;
        TierId = tierId;
        Position = position;
        CreatedAt = DateTime.UtcNow;
    }

    public void PlaceInTier(Guid? tierId, int position)
    {
        TierId = tierId;
        Position = position;
    }

    public void UpdateDetails(string imageUrl, string? caption)
    {
        ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
        Caption = caption;
    }
}
