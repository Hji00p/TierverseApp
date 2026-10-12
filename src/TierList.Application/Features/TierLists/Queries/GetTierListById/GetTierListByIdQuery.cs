using MediatR;
using TierList.Application.Features.TierLists.DTOs;

namespace TierList.Application.Features.TierLists.Queries.GetTierListById;

public record GetTierListByIdQuery(Guid Id) : IRequest<TierListDto>;
