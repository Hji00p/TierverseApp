using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.DTOs;

namespace TierList.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserRoleDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserRoleDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenException("Only administrators can view user accounts.");
        }

        return await _identityService.GetUsersAsync(request.Search, request.Page, request.PageSize, cancellationToken);
    }
}
