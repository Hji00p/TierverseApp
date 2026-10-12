using FluentValidation;

namespace TierList.Application.Features.TierLists.Commands.CreateTierList;

public class CreateTierListCommandValidator : AbstractValidator<CreateTierListCommand>
{
    public CreateTierListCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}
