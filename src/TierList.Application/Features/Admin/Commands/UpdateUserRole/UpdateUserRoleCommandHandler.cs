using FluentValidation.Results;
using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.Admin.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserRoleCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenException("Only administrators can change user roles.");
        }

        var (succeeded, errors) = await _identityService.UpdateUserRoleAsync(request.UserId, request.Role, cancellationToken);

        if (!succeeded)
        {
            var errorList = errors.ToList();
            if (errorList.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotFoundException("User", request.UserId);
            }

            throw new ValidationException(errorList.Select(e => new ValidationFailure("Role", e)));
        }
    }
}
