using MediatR;
using TierList.Application.Common.Models;
using TierList.Application.Features.TierLists.DTOs;

namespace TierList.Application.Features.TierLists.Queries.GetTierLists;

public record GetTierListsQuery(string? Search, int Page = 1, int PageSize = 20) : IRequest<PaginatedList<TierListBriefDto>>;
