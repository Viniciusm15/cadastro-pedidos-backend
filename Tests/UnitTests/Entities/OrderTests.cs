using Domain.Enums;
using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;

namespace Tests.UnitTests.Entities
{
    public class OrderTests
    {
        [Fact]
        public void Constructor_ShouldInitializeOrderItemsAsEmptyList()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            order.OrderItems.Should().NotBeNull();
            order.OrderItems.Should().BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var orderDate = DateTime.UtcNow;
            var order = new Order
            {
                OrderDate = orderDate,
                TotalValue = 100.50,
                Status = OrderStatus.Processing,
                ClientId = 1
            };

            // Act & Assert
            order.OrderDate.Should().Be(orderDate);
            order.TotalValue.Should().Be(100.50);
            order.Status.Should().Be(OrderStatus.Processing);
            order.ClientId.Should().Be(1);
            order.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Status_ShouldDefaultToPending()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            order.Status.Should().Be(OrderStatus.Pending);
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var order = new Order
            {
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Assert
            order.DeletedAt.Should().NotBeNull();
            order.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new OrderValidator();
            var order = new Order
            {
                OrderDate = DateTime.Today,
                TotalValue = 100.50,
                Status = OrderStatus.Processing,
                ClientId = 1
            };

            // Act
            var result = order.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectInvalidClientIdOrNegativeTotalValue()
        {
            // Arrange
            var validator = new OrderValidator();
            var order = new Order
            {
                ClientId = 0,
                TotalValue = -10,
                OrderDate = DateTime.MinValue
            };

            // Act
            var result = validator.Validate(order);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ClientId");
            result.Errors.Should().Contain(e => e.PropertyName == "TotalValue");
        }
    }
}
