using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(category => category.Name)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");

            RuleFor(category => category.Description)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(500).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");
        }
    }
}
