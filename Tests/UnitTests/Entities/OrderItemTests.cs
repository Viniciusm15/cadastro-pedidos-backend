using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;

namespace Tests.UnitTests.Entities
{
    public class OrderItemTests
    {
        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var order = new Order { Id = 1 };
            var product = new Product { Id = 101, Name = "Product Name", Description = "Product Description" };
            var orderItem = new OrderItem
            {
                Quantity = 2,
                UnitaryPrice = 10.99,
                OrderId = 1,
                ProductId = 101,
                Order = order,
                Product = product
            };

            // Act & Assert
            orderItem.Quantity.Should().Be(2);
            orderItem.UnitaryPrice.Should().Be(10.99);
            orderItem.OrderId.Should().Be(1);
            orderItem.ProductId.Should().Be(101);
            orderItem.Order.Should().Be(order);
            orderItem.Product.Should().Be(product);
            orderItem.IsActive.Should().BeTrue();
            orderItem.Subtotal.Should().Be(21.98);
        }

        [Fact]
        public void Subtotal_ShouldCalculateCorrectly()
        {
            // Arrange
            var orderItem = new OrderItem { Quantity = 3, UnitaryPrice = 5.50 };

            // Act & Assert
            orderItem.Subtotal.Should().Be(16.50);
        }

        [Fact]
        public void OrderIdAndOrder_ShouldBeSynchronized()
        {
            // Arrange
            var order = new Order { Id = 1 };
            var orderItem = new OrderItem { Order = order };

            // Act & Assert
            orderItem.Order.Id.Should().Be(1);
        }

        [Fact]
        public void ProductIdAndProduct_ShouldBeSynchronized()
        {
            // Arrange
            var product = new Product { Id = 101, Name = "Product Name", Description = "Product Description" };
            var orderItem = new OrderItem { Product = product };

            // Act & Assert
            orderItem.Product.Id.Should().Be(101);
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new OrderItemValidator();
            var product = new Product { Id = 101, Name = "Valid Name", Description = "Valid Description" };
            var orderItem = new OrderItem { OrderId = 1, Product = product, Quantity = 1, UnitaryPrice = 10, ProductId = product.Id };

            // Act
            var result = orderItem.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectInvalidRelationships()
        {
            // Arrange
            var validator = new OrderItemValidator();
            var orderItem = new OrderItem
            {
                Quantity = 1,
                UnitaryPrice = 10.0,
                Order = new Order { Id = 2 },
                Product = new Product { Id = 102, Name = "Product Name", Description = "Product Description" }
            };

            // Act
            var result = validator.Validate(orderItem);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Order ID is required"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Product ID is required"));
        }
    }
}
