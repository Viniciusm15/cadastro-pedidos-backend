using Domain.Enums;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class OrderControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        private readonly ProductTestHelper _productHelper = new ProductTestHelper(factory);
        private readonly OrderTestHelper _orderHelper = new OrderTestHelper(factory);

        [Fact]
        public async Task Post_CreatesNewOrder_WithProductAndImageRelationships()
        {
            // Arrange
            var product = await _productHelper.CreateTestProduct(price: 99.99);
            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderDate = DateTime.UtcNow.Date,
                Status = OrderStatus.Pending,
                TotalValue = 1,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() {
                        ProductId = product.ProductId,
                        Quantity = 2,
                        UnitaryPrice = product.Price,
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/order", newOrder);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(response);
            createdOrder.TotalValue.Should().Be(product.Price * 2);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedOrder()
        {
            // Arrange
            var product = await _productHelper.CreateTestProduct();
            var createdOrder = await _orderHelper.CreateTestOrderThroughApi(
                items: new List<OrderItemRequestModel> {
                    new()
                    {
                        ProductId = product.ProductId,
                        Quantity = 1,
                        UnitaryPrice =  product.Price ,
                    }
                });

            // Act
            var response = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await DeserializeResponse<OrderResponseModel>(response);
            order.OrderItems.Should().ContainSingle();
        }
    }
}
