using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(product => product.Name)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(100).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");

            RuleFor(product => product.Description)
                .NotEmpty().WithMessage("{PropertyName} is required");

            RuleFor(product => product.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to zero");

            RuleFor(product => product.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to zero");

            RuleFor(product => product.CategoryId)
                .GreaterThan(0).WithMessage("Category ID is required");
        }
    }
}
