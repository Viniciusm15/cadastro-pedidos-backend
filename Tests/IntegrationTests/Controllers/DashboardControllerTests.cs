using Common.Models;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class DashboardControllerTests : IntegrationTestBase
    {
        private readonly DashboardTestHelper _dashboardHelper;

        public DashboardControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dashboardHelper = new DashboardTestHelper(_client);
        }

        [Fact]
        public async Task GetDashboardMetrics_ReturnsDashboardMetrics()
        {
            // Arrange
            await _dashboardHelper.PrepareDashboardTestData();

            // Act
            var response = await _client.GetAsync("/api/dashboard/metrics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<DashboardResponseModel>(response);
            result.Should().NotBeNull();
            result.TotalSales.Should().BeGreaterThan(0);
            result.ActiveClientsCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetWeeklySalesData_ReturnsWeeklySales()
        {
            // Arrange
            await _dashboardHelper.CreateTestOrderForWeeklySales();

            // Act
            var response = await _client.GetAsync("/api/dashboard/weekly-sales");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<List<DashboardWeeklySalesResponseModel>>(response);
            result.Should().HaveCount(7);
            result.Should().Contain(x => x.TotalSales > 0);
        }

        [Fact]
        public async Task GetLowStockProducts_ReturnsPagedProducts()
        {
            // Arrange
            await _dashboardHelper.CreateLowStockProduct();

            // Act
            var response = await _client.GetAsync("/api/dashboard/low-stock-products?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<DashboardLowStockProductResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().Contain(x => x.StockQuantity < 10);
        }

        [Fact]
        public async Task GetPendingOrders_ReturnsPagedOrders()
        {
            // Arrange
            await _dashboardHelper.CreatePendingOrder();

            // Act
            var response = await _client.GetAsync("/api/dashboard/pending-orders?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<DashboardPendingOrderResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetClientsData_ReturnsClientSummary()
        {
            // Arrange
            await _dashboardHelper.CreateClientForDashboard();

            // Act
            var response = await _client.GetAsync("/api/dashboard/clients-data");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<DashboardClientSummaryResponseModel>(response);
            result.Should().NotBeNull();
            result.TotalClients.Should().BeGreaterThan(0);
            result.MonthlyData.Should().HaveCount(6);
        }

        [Fact]
        public async Task RestockProduct_WithValidData_ReturnsRestockResponse()
        {
            // Arrange
            var productId = await _dashboardHelper.CreateTestProduct(stockQuantity: 5);
            var restockQuantity = 10;

            // Act
            var response = await _client.PostAsJsonAsync($"/api/dashboard/restock-product/{productId}", restockQuantity);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<DashboardRestockResponseModel>(response);
            result.Should().NotBeNull();
            result.NewStockQuantity.Should().Be(15);
            result.Message.Should().Contain("successfully");
        }

        [Fact]
        public async Task RestockProduct_WithInvalidQuantity_ReturnsBadRequest()
        {
            // Arrange
            var productId = await _dashboardHelper.CreateTestProduct();
            var invalidQuantity = -999;

            // Act
            var response = await _client.PostAsJsonAsync($"/api/dashboard/restock-product/{productId}", invalidQuantity);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RestockProduct_WithNonExistentProduct_ReturnsNotFound()
        {
            // Arrange
            var nonExistentProductId = 9999;
            var restockQuantity = 10;

            // Act
            var response = await _client.PostAsJsonAsync($"/api/dashboard/restock-product/{nonExistentProductId}", restockQuantity);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
