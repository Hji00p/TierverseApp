using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class TemplateTier : BaseEntity
{
    public Guid TemplateId { get; private set; }
    public Template Template { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public int Order { get; private set; }

    private TemplateTier() { }

    public TemplateTier(string name, string color, int order)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Order = order;
        CreatedAt = DateTime.UtcNow;
    }

    public TemplateTier(Guid templateId, string name, string color, int order)
        : this(name, color, order)
    {
        TemplateId = templateId;
    }

    public void Update(string name, string color, int order)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Order = order;
    }
}
