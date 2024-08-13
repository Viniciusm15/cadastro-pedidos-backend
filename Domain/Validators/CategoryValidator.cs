using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(category => category.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");

            RuleFor(category => category.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("The field Description must be a maximum of 500 characters.");
        }
    }
}
