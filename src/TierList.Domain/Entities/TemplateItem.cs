using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class TemplateItem : BaseEntity
{
    public Guid TemplateId { get; private set; }
    public Template Template { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public int Order { get; private set; }

    private TemplateItem() { }

    public TemplateItem(string imageUrl, string? caption, int order)
    {
        ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
        Caption = caption;
        Order = order;
        CreatedAt = DateTime.UtcNow;
    }

    public TemplateItem(Guid templateId, string imageUrl, string? caption, int order)
        : this(imageUrl, caption, order)
    {
        TemplateId = templateId;
    }

    public void Update(string imageUrl, string? caption, int order)
    {
        ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
        Caption = caption;
        Order = order;
    }
}
