using Common.Models;
using Domain.Enums;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class OrderControllerTests : IntegrationTestBase
    {
        private readonly OrderTestHelper _orderHelper;

        public OrderControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _orderHelper = new OrderTestHelper(_client);
        }

        [Fact]
        public async Task GetAll_ReturnsPagedOrders()
        {
            // Arrange
            await _orderHelper.CreateTestOrder();

            // Act
            var getResponse = await _client.GetAsync("/api/order?pageNumber=1&pageSize=10");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<OrderResponseModel>>(getResponse);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewOrder()
        {
            // Arrange
            var orderRequestModel = await _orderHelper.CreateOrderRequestModelWithProductAndClient();

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/order", orderRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse.Headers.Location.Should().NotBeNull();

            var orderResponseModel = await DeserializeResponse<OrderResponseModel>(postResponse);
            orderResponseModel.OrderId.Should().BeGreaterThan(0);
            orderResponseModel.ClientId.Should().Be(orderRequestModel.ClientId);
            orderResponseModel.Status.Should().Be(orderRequestModel.Status);
            orderResponseModel.TotalValue.Should().Be(orderRequestModel.TotalValue);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidOrderRequestModel = _orderHelper.CreateOrderRequestModel(totalValue: 0);

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/order", invalidOrderRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedOrder()
        {
            // Arrange
            var createdOrder = await _orderHelper.CreateTestOrder();

            // Act
            var getResponse = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var orderResponseModel = await DeserializeResponse<OrderResponseModel>(getResponse);
            orderResponseModel.OrderId.Should().Be(createdOrder.OrderId);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var getResponse = await _client.GetAsync($"/api/order/{nonExistentId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingOrder()
        {
            // Arrange
            var createdOrder = await _orderHelper.CreateTestOrder();
            var updatedOrderRequestModel = _orderHelper.CreateOrderRequestModel(
                clientId: createdOrder.ClientId,
                orderDate: createdOrder.OrderDate,
                status: OrderStatus.Delivered,
                totalValue: createdOrder.TotalValue,
                items: []
            );

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/order/{createdOrder.OrderId}", updatedOrderRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");
            var orderResponseModel = await DeserializeResponse<OrderResponseModel>(getResponse);
            orderResponseModel.Status.Should().Be(OrderStatus.Delivered);
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var orderRequestModel = await _orderHelper.CreateOrderRequestModelWithProductAndClient();

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/order/{nonExistentId}", orderRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createdOrder = await _orderHelper.CreateTestOrder();
            var invalidOrderRequestModel = _orderHelper.CreateOrderRequestModel(
                clientId: createdOrder.ClientId,
                orderDate: DateTime.Today.AddDays(1),
                status: createdOrder.Status,
                totalValue: createdOrder.TotalValue,
                items: []
            );

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/order/{createdOrder.OrderId}", invalidOrderRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesOrder()
        {
            // Arrange
            var createdOrder = await _orderHelper.CreateTestOrder();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/order/{createdOrder.OrderId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/order/{nonExistentId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_GenerateOrdersCsvReport_ReturnsFile()
        {
            // Arrange
            await _orderHelper.CreateTestOrder();

            // Act
            var getResponse = await _client.GetAsync("/api/order/generate-csv-report");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            getResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

            var csvContent = await getResponse.Content.ReadAsStringAsync();
            csvContent.Should().NotBeNullOrEmpty();
        }
    }
}
