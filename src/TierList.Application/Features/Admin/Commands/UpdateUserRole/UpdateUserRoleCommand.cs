using MediatR;

namespace TierList.Application.Features.Admin.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(Guid UserId, string Role) : IRequest;
