using Domain.Enums;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using Infra.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Tests.IntegrationTests.Shared
{
    public class OrderTestHelper(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        public async Task<Order> CreateTestOrder(
            int clientId = 1,
            List<OrderItem>? items = null,
            OrderStatus status = OrderStatus.Pending,
            double? totalValue = null)
        {
            var orderItems = items ?? new List<OrderItem>();
            var calculatedTotal = totalValue ?? orderItems.Sum(i => i.Quantity * i.UnitaryPrice);

            var newOrder = new Order
            {
                ClientId = clientId,
                OrderDate = DateTime.UtcNow,
                Status = status,
                TotalValue = calculatedTotal,
                OrderItens = orderItems
            };

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Orders.Add(newOrder);
            await dbContext.SaveChangesAsync();

            return newOrder;
        }

        public async Task<Order> CreateTestOrderWithItems(
            int clientId = 1,
            List<(int productId, int quantity, double price)>? items = null,
            OrderStatus status = OrderStatus.Pending,
            double? totalValue = null)
        {
            var orderItems = items?.Select(i => new OrderItem
            {
                ProductId = i.productId,
                Quantity = i.quantity,
                UnitaryPrice = i.price
            }).ToList() ?? new List<OrderItem>();

            return await CreateTestOrder(clientId, orderItems, status, totalValue);
        }

        public async Task<OrderResponseModel> CreateTestOrderThroughApi(
            int clientId = 1,
            List<OrderItemRequestModel>? items = null,
            OrderStatus status = OrderStatus.Pending,
            double? totalValue = null)
        {
            var orderItems = items ?? new List<OrderItemRequestModel>();
            var calculatedTotal = totalValue ?? orderItems.Sum(i => i.Quantity * i.UnitaryPrice);

            var orderRequest = new OrderRequestModel
            {
                ClientId = clientId,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = calculatedTotal,
                Status = status,
                OrderItems = orderItems
            };

            var response = await _client.PostAsJsonAsync("/api/order", orderRequest);
            return await DeserializeResponse<OrderResponseModel>(response);
        }

        public async Task EnsureOrderExists(
            int orderId,
            int clientId = 1,
            double totalValue = 0)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingOrder = await dbContext.Orders.FindAsync(orderId);
            if (existingOrder == null)
            {
                dbContext.Orders.Add(new Order
                {
                    Id = orderId,
                    ClientId = clientId,
                    OrderDate = DateTime.UtcNow,
                    TotalValue = totalValue,
                    Status = OrderStatus.Pending
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
