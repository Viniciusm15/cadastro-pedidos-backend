using Application.Interfaces;
using Application.Services;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class OrderItemServiceTests
    {
        private readonly Mock<ILogger<OrderItemService>> _loggerMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IValidator<OrderItem>> _orderItemValidatorMock;
        private readonly OrderItemService _orderItemService;

        public OrderItemServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrderItemService>>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _productServiceMock = new Mock<IProductService>();
            _orderItemValidatorMock = new Mock<IValidator<OrderItem>>();

            _orderItemService = new OrderItemService(
                _loggerMock.Object,
                _orderItemRepositoryMock.Object,
                _productServiceMock.Object,
                _orderItemValidatorMock.Object);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_ShouldReturnOrderItems_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var orderItems = new List<OrderItem>
            {
                new() {
                    Id = 1,
                    OrderId = orderId,
                    Quantity = 2,
                    UnitaryPrice = 10.50,
                    ProductId = 1,
                    Product = new Product { Id = 1, Name = "Product 1", Description = "Description 1" }
                },
                new() {
                    Id = 2,
                    OrderId = orderId,
                    Quantity = 1,
                    UnitaryPrice = 15.75,
                    ProductId = 2,
                    Product = new Product { Id = 2, Name = "Product 2", Description = "Description 2" }
                }
            };

            _orderItemRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId))
                .ReturnsAsync(orderItems);

            // Act
            var result = await _orderItemService.GetOrderItemsByOrderId(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().OrderItemId.Should().Be(1);
            result.First().ProductName.Should().Be("Product 1");
            result.Last().OrderItemId.Should().Be(2);
            result.Last().ProductName.Should().Be("Product 2");

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieving all order itens")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved 2 order itens")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrderItem_ShouldReturnCreatedOrderItem_WhenValidationPasses()
        {
            // Arrange
            var requestModel = new OrderItemRequestModel
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 3,
                UnitaryPrice = 12.99
            };

            var productResponse = new ProductResponseModel
            {
                Name = "Test Product",
                Price = 12.99,
                Description = "Test Description",
                CategoryId = 1
            };

            var createdOrderItem = new OrderItem
            {
                Id = 1,
                OrderId = requestModel.OrderId,
                ProductId = requestModel.ProductId,
                Quantity = requestModel.Quantity,
                UnitaryPrice = requestModel.UnitaryPrice,
            };

            _orderItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<OrderItem>()))
                .Returns(new ValidationResult());

            _productServiceMock
                .Setup(x => x.GetProductById(requestModel.ProductId))
                .ReturnsAsync(productResponse);

            _orderItemRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<OrderItem>()))
                .Callback<OrderItem>(i => i.Id = createdOrderItem.Id)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderItemService.CreateOrderItem(requestModel);

            // Assert
            result.Should().NotBeNull();
            result.OrderItemId.Should().Be(createdOrderItem.Id);
            result.OrderId.Should().Be(requestModel.OrderId);
            result.ProductId.Should().Be(requestModel.ProductId);
            result.Quantity.Should().Be(requestModel.Quantity);
            result.UnitaryPrice.Should().Be(requestModel.UnitaryPrice);
            result.Subtotal.Should().Be(requestModel.Quantity * requestModel.UnitaryPrice);
            result.ProductName.Should().Be(productResponse.Name);

            _orderItemRepositoryMock.Verify(x => x.CreateAsync(It.Is<OrderItem>(i =>
                i.OrderId == requestModel.OrderId &&
                i.ProductId == requestModel.ProductId &&
                i.Quantity == requestModel.Quantity &&
                i.UnitaryPrice == requestModel.UnitaryPrice)),
            Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting order item creation")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order item created with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrderItem_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var requestModel = new OrderItemRequestModel
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 0, // Invalid quantity
                UnitaryPrice = 12.99
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Quantity", "Quantity must be greater than 0")
            };

            _orderItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<OrderItem>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _orderItemService.CreateOrderItem(requestModel));

            exception.ValidationErrors.Should().Contain("Quantity must be greater than 0");

            _orderItemRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<OrderItem>()), Times.Never);

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order item creation failed due to validation errors")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrderItem_ShouldUpdateOrderItem_WhenValidationPasses()
        {
            // Arrange
            var orderItemId = 1;
            var requestModel = new OrderItemRequestModel
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 5,
                UnitaryPrice = 15.99
            };

            var existingOrderItem = new OrderItem
            {
                Id = orderItemId,
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitaryPrice = 12.99,
            };

            _orderItemRepositoryMock
                .Setup(x => x.GetByIdAsync(orderItemId))
                .ReturnsAsync(existingOrderItem);

            _orderItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<OrderItem>()))
                .Returns(new ValidationResult());

            // Act
            await _orderItemService.UpdateOrderItem(orderItemId, requestModel);

            // Assert
            _orderItemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<OrderItem>(i =>
                i.Id == orderItemId &&
                i.Quantity == requestModel.Quantity &&
                i.UnitaryPrice == requestModel.UnitaryPrice &&
                i.Subtotal == requestModel.Quantity * requestModel.UnitaryPrice)),
            Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting order item update")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order item updated with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrderItem_ShouldThrowNotFoundException_WhenOrderItemDoesNotExist()
        {
            // Arrange
            var orderItemId = 999;
            var requestModel = new OrderItemRequestModel
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 1,
                UnitaryPrice = 10.00
            };

            _orderItemRepositoryMock
                .Setup(x => x.GetByIdAsync(orderItemId))
                .ReturnsAsync((OrderItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _orderItemService.UpdateOrderItem(orderItemId, requestModel));

            _orderItemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrderItem>()), Times.Never);

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Order item not found by ID: {orderItemId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrderItem_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var orderItemId = 999;
            var requestModel = new OrderItemRequestModel
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 1,
                UnitaryPrice = -1 // Invalid UnityPrice
            };

            var existingOrderItem = new OrderItem
            {
                Id = orderItemId,
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitaryPrice = 1,
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("UnitaryPrice", "Invalid UnityPrice")
            };

            _orderItemRepositoryMock
               .Setup(x => x.GetByIdAsync(orderItemId))
               .ReturnsAsync(existingOrderItem);

            _orderItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<OrderItem>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _orderItemService.UpdateOrderItem(orderItemId, requestModel));

            exception.ValidationErrors.Should().Contain("Invalid UnityPrice");

            _orderItemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task SyncOrderItems_ShouldAddUpdateAndRemoveItems_WhenItemsExist()
        {
            // Arrange
            var orderId = 1;
            var existingItems = new List<OrderItem>
            {
                new() { Id = 1, OrderId = orderId, ProductId = 1, Quantity = 2 },
                new() { Id = 2, OrderId = orderId, ProductId = 2, Quantity = 1 }
            };

            var itemRequests = new List<OrderItemRequestModel>
            {
                new() { Id = 1, OrderId = orderId, ProductId = 1, Quantity = 3 }, // Update
                new() { Id = 0, OrderId = orderId, ProductId = 3, Quantity = 1 }  // Add
            };

            var productResponse = new ProductResponseModel
            {
                Name = "Test Product",
                Price = 12.99,
                Description = "Test Description",
                CategoryId = 1
            };

            // Configurar o mock para GetByIdAsync (usado no UpdateOrderItem)
            _orderItemRepositoryMock
                .Setup(x => x.GetByIdAsync(1)) // Específico para o item existente
                .ReturnsAsync(existingItems[0]);

            _orderItemRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId))
                .ReturnsAsync(existingItems);

            _orderItemValidatorMock
                .Setup(x => x.Validate(It.IsAny<OrderItem>()))
                .Returns(new ValidationResult());

            _productServiceMock
                .Setup(x => x.GetProductById(It.IsAny<int>()))
                .ReturnsAsync(productResponse);

            // Configurar o mock para atribuir ID ao novo item
            _orderItemRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<OrderItem>()))
                .Callback<OrderItem>(item => item.Id = 3)
                .Returns(Task.CompletedTask);

            // Act
            await _orderItemService.SyncOrderItems(orderId, itemRequests);

            // Assert
            _orderItemRepositoryMock.Verify(x => x.DeleteAsync(
                It.Is<OrderItem>(i => i.Id == 2)),
                Times.Once);

            _orderItemRepositoryMock.Verify(x => x.UpdateAsync(
                It.Is<OrderItem>(i =>
                    i.Id == 1 &&
                    i.Quantity == 3)),
                Times.Once);

            _orderItemRepositoryMock.Verify(x => x.CreateAsync(
                It.Is<OrderItem>(i =>
                    i.OrderId == orderId &&
                    i.ProductId == 3 &&
                    i.Quantity == 1)),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting sync of order items")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing order item with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order items synced for Order ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrderItem_ShouldDeleteOrderItem_WhenItemExists()
        {
            // Arrange
            var orderItemId = 1;
            var existingOrderItem = new OrderItem
            {
                Id = orderItemId,
                OrderId = 1,
                ProductId = 1,
                Quantity = 1
            };

            _orderItemRepositoryMock
                .Setup(x => x.GetByIdAsync(orderItemId))
                .ReturnsAsync(existingOrderItem);

            // Act
            await _orderItemService.DeleteOrderItem(orderItemId);

            // Assert
            _orderItemRepositoryMock.Verify(x => x.DeleteAsync(existingOrderItem), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting order item with ID: {orderItemId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleted order item ID: {orderItemId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrderItem_ShouldThrowNotFoundException_WhenItemDoesNotExist()
        {
            // Arrange
            var orderItemId = 999;

            _orderItemRepositoryMock
                .Setup(x => x.GetByIdAsync(orderItemId))
                .ReturnsAsync((OrderItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _orderItemService.DeleteOrderItem(orderItemId));

            _orderItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<OrderItem>()), Times.Never);

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Order item not found by ID: {orderItemId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
