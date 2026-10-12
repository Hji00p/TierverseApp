using FluentValidation;

namespace TierList.Application.Features.Votes.Commands.VoteComment;

public class VoteCommentCommandValidator : AbstractValidator<VoteCommentCommand>
{
    public VoteCommentCommandValidator()
    {
        RuleFor(v => v.CommentId)
            .NotEmpty().WithMessage("CommentId is required.");

        RuleFor(v => v.Value)
            .Must(v => v == 1 || v == -1 || v == 0)
            .WithMessage("Vote value must be 1 (Like), -1 (Dislike), or 0 (Remove vote).");
    }
}
