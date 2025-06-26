using Domain.Enums;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;

namespace Tests.IntegrationTests.Shared
{
    public class OrderTestHelper(HttpClient client)
    {
        private readonly HttpClient _client = client;
        private readonly ProductTestHelper _productHelper = new(client);
        private readonly ClientTestHelper _clientHelper = new(client);

        public OrderRequestModel CreateOrderRequestModel(
            int clientId = 0,
            DateTime? orderDate = null,
            OrderStatus status = OrderStatus.Pending,
            double totalValue = 0,
            List<OrderItemRequestModel>? items = null)
        {
            return new OrderRequestModel
            {
                ClientId = clientId,
                OrderDate = orderDate ?? DateTime.UtcNow.Date,
                Status = status,
                TotalValue = totalValue,
                OrderItems = items ?? []
            };
        }

        public async Task<OrderRequestModel> CreateOrderRequestModelWithProductAndClient(
            OrderStatus status = OrderStatus.Pending,
            int quantity = 2,
            double? unitaryPrice = null)
        {
            var product = await _productHelper.CreateTestProduct(price: unitaryPrice ?? 20.0);
            var client = await _clientHelper.CreateTestClient();
            var total = (unitaryPrice ?? product.Price) * quantity;

            return CreateOrderRequestModel(
                clientId: client.ClientId,
                orderDate: DateTime.UtcNow.Date.AddDays(-1),
                status: status,
                totalValue: total,
                items:
                [
                    new OrderItemRequestModel
                    {
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitaryPrice = product.Price
                    }
                ]);
        }

        public async Task<OrderResponseModel> CreateTestOrder(
            int? clientId = null,
            OrderStatus status = OrderStatus.Pending,
            int quantity = 2,
            double? unitaryPrice = null)
        {
            var product = await _productHelper.CreateTestProduct(price: unitaryPrice ?? 20.0);
            var client = await _clientHelper.CreateTestClient();

            var total = (unitaryPrice ?? product.Price) * quantity;

            var orderRequestModel = CreateOrderRequestModel(
                clientId: client.ClientId,
                orderDate: DateTime.UtcNow.Date.AddDays(-1),
                status: status,
                totalValue: total,
                items:
                [
                    new OrderItemRequestModel
                    {
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitaryPrice = product.Price
                    }
                ]);

            var postResponse = await _client.PostAsJsonAsync("/api/order", orderRequestModel);
            return await IntegrationTestBase.DeserializeResponse<OrderResponseModel>(postResponse);
        }
    }
}
