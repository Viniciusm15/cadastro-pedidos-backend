using Application.Interfaces;
using Application.Services;
using Common.Exceptions;
using Common.Helpers;
using Common.Models;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.Reports;
using Domain.Models.RequestModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IValidator<Order>> _orderValidatorMock;
        private readonly Mock<IOrderItemService> _orderItemServiceMock;
        private readonly Mock<ICsvService> _csvServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrderService>>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderValidatorMock = new Mock<IValidator<Order>>();
            _orderItemServiceMock = new Mock<IOrderItemService>();
            _csvServiceMock = new Mock<ICsvService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _orderService = new OrderService(
                _loggerMock.Object,
                _orderRepositoryMock.Object,
                _orderValidatorMock.Object,
                _orderItemServiceMock.Object,
                _csvServiceMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnPagedOrders()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var orders = new List<Order>
            {
                new() { Id = 1, OrderDate = DateTime.Now, TotalValue = 100, Status = OrderStatus.Pending, ClientId = 1 },
                new() { Id = 2, OrderDate = DateTime.Now.AddDays(-1), TotalValue = 200, Status = OrderStatus.Delivered, ClientId = 2 }
            };

            var pagedResult = new PagedResult<Order>(orders, 2);

            _orderRepositoryMock
                .Setup(x => x.GetAllOrdersAsync(pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _orderService.GetAllOrders(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.First().OrderId.Should().Be(1);
            result.Items.Last().OrderId.Should().Be(2);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieving orders for page {pageNumber}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var expectedOrder = new Order
            {
                Id = orderId,
                OrderDate = DateTime.Now,
                TotalValue = 150.50,
                Status = OrderStatus.Pending,
                ClientId = 1
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _orderService.GetOrderById(orderId);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(orderId);
            result.TotalValue.Should().Be(expectedOrder.TotalValue);
            result.Status.Should().Be(expectedOrder.Status);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Starting order search with ID {orderId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderById_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 999;

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _orderService.GetOrderById(orderId));

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Order not found by ID: {orderId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreatedOrder_WhenValidationPasses()
        {
            // Arrange
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = 100,
                Status = OrderStatus.Pending,
                ClientId = 1,
                OrderItems =
                [
                    new() { ProductId = 1, Quantity = 2, UnitaryPrice = 25 },
            new() { ProductId = 2, Quantity = 1, UnitaryPrice = 50 }
                ]
            };

            var createdOrder = new Order
            {
                Id = 1,
                OrderDate = requestModel.OrderDate,
                TotalValue = requestModel.TotalValue,
                Status = requestModel.Status,
                ClientId = requestModel.ClientId
            };

            _orderValidatorMock
                .Setup(x => x.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

            _orderRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .Callback<Order>(o => o.Id = createdOrder.Id)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.CreateOrder(requestModel);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(createdOrder.Id);
            result.Status.Should().Be(requestModel.Status);

            // Verify order items were created
            _orderItemServiceMock.Verify(
                x => x.CreateOrderItem(It.Is<OrderItemRequestModel>(i =>
                    i.ProductId == 1 || i.ProductId == 2)),
                Times.Exactly(2));

            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Order created with ID: {createdOrder.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = -1, // Invalid TotalValue
                Status = OrderStatus.Pending,
                ClientId = 1,
                OrderItems =
                [
                    new() { ProductId = 1, Quantity = 2, UnitaryPrice = 25 },
                    new() { ProductId = 2, Quantity = 1, UnitaryPrice = 50 }
                ]
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("TotalValue", "Total value must be greater than zero")
            };

            _orderValidatorMock
               .Setup(x => x.Validate(It.IsAny<Order>()))
               .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _orderService.CreateOrder(requestModel));

            exception.ValidationErrors.Should().Contain("Total value must be greater than zero");

            _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order creation failed due to validation errors")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = 100,
                Status = OrderStatus.Pending,
                ClientId = 1,
                OrderItems = new List<OrderItemRequestModel>()
            };

            _orderValidatorMock
                .Setup(x => x.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

            _orderRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .ThrowsAsync(new Exception("Test exception"));

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.RollbackAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrder(requestModel));

            // Verify rollback was called
            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order creation failed. Rolling back transaction")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ShouldUpdateOrder_WhenValidationPasses()
        {
            // Arrange
            var orderId = 1;
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = 200,
                Status = OrderStatus.Delivered,
                ClientId = 1,
                OrderItems =
                [
                    new() { Id = 1, ProductId = 1, Quantity = 3, UnitaryPrice = 25 },
                    new() { Id = 0, ProductId = 2, Quantity = 2, UnitaryPrice = 50 }
                ]
            };

            var existingOrder = new Order
            {
                Id = orderId,
                OrderDate = DateTime.Now.AddDays(-1),
                TotalValue = 100,
                Status = OrderStatus.Pending,
                ClientId = 1,
                OrderItens =
                [
                    new() { Id = 1, ProductId = 1, Quantity = 2, UnitaryPrice = 25 }
                ]
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync(existingOrder);

            _orderValidatorMock
                .Setup(x => x.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync())
                .Returns(Task.CompletedTask);

            // Setup UpdateAsync to return completed task
            _orderRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _orderService.UpdateOrder(orderId, requestModel);

            // Assert
            _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);

            _orderItemServiceMock.Verify(x => x.SyncOrderItems(orderId, requestModel.OrderItems), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order updated with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 1;
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = 200,
                Status = OrderStatus.Delivered,
                ClientId = 1,
                OrderItems =
                [
                    new() { Id = 1, ProductId = 1, Quantity = 3, UnitaryPrice = 25 },
                    new() { Id = 0, ProductId = 2, Quantity = 2, UnitaryPrice = 50 }
                ]
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _orderService.UpdateOrder(orderId, requestModel));

            _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var orderId = 1;
            var requestModel = new OrderRequestModel
            {
                OrderDate = DateTime.Now,
                TotalValue = -1,
                Status = OrderStatus.Delivered,
                ClientId = 1,
                OrderItems =
                [
                    new() { Id = 1, ProductId = 1, Quantity = 3, UnitaryPrice = 25 },
                    new() { Id = 0, ProductId = 2, Quantity = 2, UnitaryPrice = 50 }
                ]
            };

            var existingOrder = new Order
            {
                Id = orderId,
                OrderDate = DateTime.Now,
                TotalValue = 200,
                Status = OrderStatus.Delivered,
                ClientId = 1,
                OrderItens = []
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("TotalValue", "Total value must be greater than zero")
            };

            _orderRepositoryMock
               .Setup(x => x.GetOrderByIdAsync(orderId))
               .ReturnsAsync(existingOrder);

            _orderValidatorMock
                .Setup(x => x.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _orderService.UpdateOrder(orderId, requestModel));

            exception.ValidationErrors.Should().Contain("Total value must be greater than zero");

            _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrder_ShouldCancelOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var existingOrder = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                OrderItens = new List<OrderItem>
                {
                    new() { Id = 1, ProductId = 1, Quantity = 2 },
                    new() { Id = 2, ProductId = 2, Quantity = 1 }
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync(existingOrder);

            // Act
            await _orderService.DeleteOrder(orderId);

            // Assert
            _orderItemServiceMock.Verify(x => x.DeleteOrderItem(1), Times.Once);
            _orderItemServiceMock.Verify(x => x.DeleteOrderItem(2), Times.Once);

            _orderRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Order>(o =>
                o.Status == OrderStatus.Canceled)),
                Times.Once);

            _orderRepositoryMock.Verify(x => x.DeleteAsync(existingOrder), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order status updated to 'Canceled'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 999;

            _orderRepositoryMock
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _orderService.DeleteOrder(orderId));

            _orderRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task GenerateOrdersReportCsvAsync_ShouldReturnCsvData_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new()
                {
                    Id = 1,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Delivered,
                    TotalValue = 100,
                    Client = new Client {
                        Name = "Test Client",
                        Email = "client1@test.com",
                        Telephone = "123456789"
                    },
                    OrderItens = new List<OrderItem>
                    {
                        new() { Quantity = 2 },
                        new() { Quantity = 1 }
                    }
                }
            };

            var expectedCsvData = new byte[] { 0x01, 0x02, 0x03 };

            _orderRepositoryMock
                .Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            _csvServiceMock
                .Setup(x => x.WriteCsvToByteArray(It.IsAny<List<OrderReportModel>>()))
                .Returns(expectedCsvData);

            // Act
            var result = await _orderService.GenerateOrdersReportCsvAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().Equal(expectedCsvData);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CSV report data generated successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateOrdersReportCsvAsync_ShouldReturnEmptyArray_WhenNoOrdersExist()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _orderService.GenerateOrdersReportCsvAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            // Verify warning logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orders found to generate the report")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateOrdersReportCsvAsync_ShouldHandleException_WhenCsvGenerationFails()
        {
            // Arrange
            var orders = new List<Order> { new() { Id = 1 } };
            var exception = new Exception("CSV generation failed");

            _orderRepositoryMock
                .Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            _csvServiceMock
                .Setup(x => x.WriteCsvToByteArray(It.IsAny<List<OrderReportModel>>()))
                .Throws(exception);

            // Act
            var result = await _orderService.GenerateOrdersReportCsvAsync();

            // Assert
            Assert.Empty(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while generating the CSV report")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
