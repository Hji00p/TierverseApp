using FluentValidation;

namespace TierList.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(v => v.Tiers)
            .NotEmpty().WithMessage("At least one tier definition is required.");

        RuleForEach(v => v.Tiers).ChildRules(tier =>
        {
            tier.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tier name is required.")
                .MaximumLength(50).WithMessage("Tier name must not exceed 50 characters.");

            tier.RuleFor(t => t.Color)
                .NotEmpty().WithMessage("Tier color is required.")
                .MaximumLength(30).WithMessage("Tier color must not exceed 30 characters.");
        });

        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ImageUrl)
                .NotEmpty().WithMessage("Item image URL is required.")
                .MaximumLength(2000).WithMessage("Image URL must not exceed 2000 characters.");

            item.RuleFor(i => i.Caption)
                .MaximumLength(100).WithMessage("Caption must not exceed 100 characters.");
        });
    }
}
