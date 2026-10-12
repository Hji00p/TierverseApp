using FluentValidation;

namespace TierList.Application.Features.Admin.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    private static readonly string[] AllowedRoles = ["User", "Moderator", "Admin"];

    public UpdateUserRoleCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(v => v.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => AllowedRoles.Contains(role))
            .WithMessage("Role must be one of: User, Moderator, Admin.");
    }
}
