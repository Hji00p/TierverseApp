using MediatR;
using TierList.Application.Features.TierLists.DTOs;

namespace TierList.Application.Features.TierLists.Commands.CreateTierList;

public record CreateTierListCommand(
    string Title,
    string? Description = null,
    Guid? TemplateId = null,
    List<CreateTierDto>? Tiers = null,
    List<CreateTierItemDto>? Items = null,
    SaveLayoutDto? Layout = null
) : IRequest<Guid>
{
    public CreateTierListCommand(
        string title,
        string? description,
        List<CreateTierDto>? tiers,
        List<CreateTierItemDto>? items)
        : this(title, description, null, tiers, items, null)
    {
    }
}
