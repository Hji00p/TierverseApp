using FluentValidation;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierListLayout;

public class UpdateTierListLayoutCommandValidator : AbstractValidator<UpdateTierListLayoutCommand>
{
    public UpdateTierListLayoutCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Tier list ID is required.");

        RuleFor(v => v.Layout)
            .NotNull().WithMessage("Layout cannot be null.");

        When(v => v.Layout != null, () =>
        {
            RuleFor(v => v.Layout.Tiers)
                .NotNull().WithMessage("Tiers layout cannot be null.");

            RuleFor(v => v.Layout.UnplacedItemIds)
                .NotNull().WithMessage("Unplaced items list cannot be null.");
        });
    }
}
