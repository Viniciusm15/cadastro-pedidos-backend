using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class OrderItemValidator : AbstractValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(orderItem => orderItem.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            RuleFor(orderItem => orderItem.UnitaryPrice)
                .GreaterThan(0).WithMessage("Unitary price must be greater than zero");

            RuleFor(orderItem => orderItem.OrderId)
                .GreaterThan(0).WithMessage("Order ID is required");

            RuleFor(orderItem => orderItem.ProductId)
                .GreaterThan(0).WithMessage("Product ID is required");
        }
    }
}
