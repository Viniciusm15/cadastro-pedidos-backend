using Application.Interfaces;
using Application.Services;
using Common.Models;
using Domain.Enums;
using Domain.Models.ResponseModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<ILogger<DashboardService>> _loggerMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _loggerMock = new Mock<ILogger<DashboardService>>();
            _orderServiceMock = new Mock<IOrderService>();
            _productServiceMock = new Mock<IProductService>();
            _clientServiceMock = new Mock<IClientService>();

            _dashboardService = new DashboardService(
                _loggerMock.Object,
                _orderServiceMock.Object,
                _productServiceMock.Object,
                _clientServiceMock.Object);
        }

        [Fact]
        public async Task GetDashboardMetricsAsync_ShouldReturnCompleteDashboardMetrics()
        {
            // Arrange
            var expectedTotalSales = 15000.50;
            var expectedSalesChange = 10;
            var expectedLowStockCount = 5;
            var expectedPendingOrders = 3;
            var expectedActiveClients = 25;
            var expectedNewClients = 5;

            _orderServiceMock.Setup(x => x.GetTotalOrderSalesAsync()).ReturnsAsync(expectedTotalSales);
            _orderServiceMock.Setup(x => x.GetOrderSalesTrendAsync())
                .ReturnsAsync((expectedTotalSales, 13500.45, expectedSalesChange));
            _productServiceMock.Setup(x => x.GetLowStockProductsCountAsync(It.IsAny<int>())).ReturnsAsync(expectedLowStockCount);
            _orderServiceMock.Setup(x => x.GetPendingOrdersCountAsync()).ReturnsAsync(expectedPendingOrders);
            _clientServiceMock.Setup(x => x.GetActiveClientsCountAsync(It.IsAny<int>())).ReturnsAsync(expectedActiveClients);
            _clientServiceMock.Setup(x => x.GetNewClientsThisMonthAsync()).ReturnsAsync(expectedNewClients);

            // Act
            var result = await _dashboardService.GetDashboardMetricsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalSales.Should().Be(expectedTotalSales);
            result.SalesChangePercentage.Should().Be(expectedSalesChange);
            result.LowStockProductsCount.Should().Be(expectedLowStockCount);
            result.PendingOrdersCount.Should().Be(expectedPendingOrders);
            result.ActiveClientsCount.Should().Be(expectedActiveClients);
            result.NewClientsThisMonth.Should().Be(expectedNewClients);
        }

        [Fact]
        public async Task GetWeeklySalesDataAsync_ShouldReturnCorrectWeeklyData()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);

            var orders = new List<OrderResponseModel>
            {
                new() { OrderDate = new DateTime(2023, 1, 2), TotalValue = 100 }, 
                new() { OrderDate = new DateTime(2023, 1, 2), TotalValue = 200 }, 
                new() { OrderDate = new DateTime(2023, 1, 4), TotalValue = 150 }, 
                new() { OrderDate = new DateTime(2023, 1, 4), TotalValue = 50 }, 
            };

            _orderServiceMock.Setup(x => x.GetOrdersByDateRangeAsync(startDate, endDate))
                .ReturnsAsync(orders);

            // Act
            var result = await _dashboardService.GetWeeklySalesDataAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(7);

            var monday = result.First(x => x.DayOfWeek == "Seg");
            monday.TotalSales.Should().Be(300); // 100 + 200

            var wednesday = result.First(x => x.DayOfWeek == "Qua");
            wednesday.TotalSales.Should().Be(200); // 150 + 50

            // Days with no sales should be zero
            result.Where(x => x.DayOfWeek != "Seg" && x.DayOfWeek != "Qua")
                  .All(x => x.TotalSales == 0)
                  .Should().BeTrue();

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Getting weekly sales data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnPagedProducts()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var threshold = 10;
            var expectedProducts = new PagedResult<DashboardLowStockProductResponseModel>(
                [
                    new() { Id = 1, ProductName = "Product 1", StockQuantity = 3, CategoryName = "Category 1" }
                ], 1);

            _productServiceMock.Setup(x => x.GetLowStockProductsAsync(pageNumber, pageSize, threshold))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _dashboardService.GetLowStockProductsAsync(pageNumber, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Getting low stock products for page {pageNumber} with size {pageSize}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPendingOrdersAsync_ShouldReturnPagedOrders()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var expectedOrders = new PagedResult<DashboardPendingOrderResponseModel>(
                [
                    new() { Id = "ORD-1", ClientName = "Client 1", Amount = 100, Status = OrderStatus.Pending }
                ], 1);

            _orderServiceMock.Setup(x => x.GetPendingOrdersAsync(pageNumber, pageSize))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _dashboardService.GetPendingOrdersAsync(pageNumber, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedOrders);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Getting pending orders for page {pageNumber} with size {pageSize}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetClientDataAsync_ShouldReturnClientSummary()
        {
            // Arrange
            var expectedData = new DashboardClientSummaryResponseModel
            {
                TotalClients = 50,
                NewClientsThisMonth = 5,
                RetentionRate = 85,
                MonthlyData = new List<int> { 40, 42, 45, 47, 48, 50 }
            };

            _clientServiceMock.Setup(x => x.GetClientDataAsync())
                .ReturnsAsync(expectedData);

            // Act
            var result = await _dashboardService.GetClientDataAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedData);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting client dashboard data")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestockProductAsync_ShouldReturnRestockResponse()
        {
            // Arrange
            var productId = 1;
            var restockQuantity = 10;
            var expectedResponse = new DashboardRestockResponseModel
            {
                Message = "Stock replenished successfully",
                NewStockQuantity = 25
            };

            _productServiceMock.Setup(x => x.RestockProductAsync(productId, restockQuantity))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _dashboardService.RestockProductAsync(productId, restockQuantity);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Processing restock for product {productId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestockProductAsync_ShouldPropagateException_WhenServiceFails()
        {
            // Arrange
            var productId = 1;
            var restockQuantity = 10;
            var exception = new Exception("Test exception");

            _productServiceMock.Setup(x => x.RestockProductAsync(productId, restockQuantity))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _dashboardService.RestockProductAsync(productId, restockQuantity));
        }
    }
}