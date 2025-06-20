using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class OrderItemControllerTests : IntegrationTestBase
    {
        private readonly ProductTestHelper _productHelper;
        private readonly OrderItemTestHelper _orderItemHelper;

        public OrderItemControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _productHelper = new ProductTestHelper(factory);
            _orderItemHelper = new OrderItemTestHelper(factory);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_ReturnsOrderItems()
        {
            // Arrange
            var createdProduct = await _productHelper.CreateTestProduct();

            await _orderItemHelper.EnsureOrderExists(1);

            var newOrderItem = await _orderItemHelper.CreateTestOrderItem(1, createdProduct.ProductId, 10, 2);

            // Act
            var response = await _client.GetAsync($"/api/orderitem/{newOrderItem.OrderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeResponse<IEnumerable<OrderItemResponseModel>>(response);
            result.Should().NotBeEmpty();
            result.Should().Contain(x => x.ProductId == createdProduct.ProductId);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_WithNonExistentOrderId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentOrderId = 9999;

            // Act
            var response = await _client.GetAsync($"/api/orderitem/{nonExistentOrderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
