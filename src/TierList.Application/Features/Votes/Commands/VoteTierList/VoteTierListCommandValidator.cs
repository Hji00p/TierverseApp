using FluentValidation;

namespace TierList.Application.Features.Votes.Commands.VoteTierList;

public class VoteTierListCommandValidator : AbstractValidator<VoteTierListCommand>
{
    public VoteTierListCommandValidator()
    {
        RuleFor(v => v.TierListId)
            .NotEmpty().WithMessage("TierListId is required.");

        RuleFor(v => v.Value)
            .Must(v => v == 1 || v == -1 || v == 0)
            .WithMessage("Vote value must be 1 (Like), -1 (Dislike), or 0 (Remove vote).");
    }
}
