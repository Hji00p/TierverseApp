using MediatR;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierList;

public record UpdateTierListCommand(
    Guid Id,
    string Title,
    string? Description = null
) : IRequest;
