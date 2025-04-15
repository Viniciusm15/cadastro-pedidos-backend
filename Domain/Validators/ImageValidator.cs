using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ImageValidator : AbstractValidator<Image>
    {
        public ImageValidator()
        {
            RuleFor(image => image.ImageData)
                .NotEmpty()
                .WithMessage("Image data is required.")
                .Must(imageData => imageData.Length > 0)
                .WithMessage("Image data cannot be empty.");

            RuleFor(image => image.ImageMimeType)
                .NotEmpty()
                .WithMessage("MIME type is required.")
                .Must(mimeType => mimeType.StartsWith("image/"))
                .WithMessage("MIME type must be a valid image type (e.g., image/jpeg, image/png).");

            RuleFor(image => image.Description)
                .MaximumLength(255)
                .WithMessage("{PropertyName} cannot be longer than {MaxLength} characters.");
        }
    }
}
