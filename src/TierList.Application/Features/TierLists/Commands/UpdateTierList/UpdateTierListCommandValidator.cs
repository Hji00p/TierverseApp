using FluentValidation;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierList;

public class UpdateTierListCommandValidator : AbstractValidator<UpdateTierListCommand>
{
    public UpdateTierListCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Tier list ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}
