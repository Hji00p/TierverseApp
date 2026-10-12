using MediatR;
using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.DTOs;

namespace TierList.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery(string? Search = null, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedList<UserRoleDto>>;
