using MediatR;

namespace TierList.Application.Features.TierLists.Commands.DeleteTierList;

public record DeleteTierListCommand(Guid Id) : IRequest;
