using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(order => order.OrderDate)
                .NotEmpty().WithMessage("Order date is required")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Order date cannot be in the future");

            RuleFor(order => order.TotalValue)
                .GreaterThan(0).WithMessage("Total value must be greater than zero");

            RuleFor(order => order.ClientId)
                .GreaterThan(0).WithMessage("Client ID is required");
        }
    }
}
