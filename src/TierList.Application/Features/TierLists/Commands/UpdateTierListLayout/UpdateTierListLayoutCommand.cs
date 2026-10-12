using MediatR;
using TierList.Application.Features.TierLists.DTOs;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierListLayout;

public record UpdateTierListLayoutCommand(
    Guid Id,
    SaveLayoutDto Layout
) : IRequest;
