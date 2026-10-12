using FluentValidation;

namespace TierList.Application.Features.Images.Commands.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/jpg",
        "image/pjpeg",
        "image/png",
        "image/x-png",
        "image/webp",
        "image/gif",
        "image/bmp",
        "image/avif"
    };

    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Image file cannot be empty.")
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage("Image file size must not exceed 10 MB.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Image content type is required.")
            .Must(ct => AllowedContentTypes.Contains(ct?.ToLowerInvariant()))
            .WithMessage("Only standard image formats (JPEG, PNG, WebP, GIF, BMP, AVIF) are allowed.");
    }
}
