using FluentValidation;

namespace TierList.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.TierListId)
            .NotEmpty().WithMessage("TierListId is required.");

        RuleFor(v => v.Text)
            .NotEmpty().WithMessage("Comment text is required.")
            .MaximumLength(2000).WithMessage("Comment text must not exceed 2000 characters.");
    }
}
