using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class OrderItemControllerTests : IntegrationTestBase
    {
        private readonly OrderTestHelper _orderHelper;

        public OrderItemControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _orderHelper = new OrderTestHelper(_client);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_ReturnsOrderItems()
        {
            // Arrange
            var createdOrder = await _orderHelper.CreateTestOrder();

            // Act
            var getResponse = await _client.GetAsync($"/api/orderItem/{createdOrder.OrderId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var orderItemResponseModels = await DeserializeResponse<IEnumerable<OrderItemResponseModel>>(getResponse);
            orderItemResponseModels.Should().NotBeNull();
            orderItemResponseModels.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_WithNonExistentOrderId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentOrderId = 9999;

            // Act
            var getResponse = await _client.GetAsync($"/api/orderItem/{nonExistentOrderId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
