using Domain.Models.Entities;
using Infra.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.IntegrationTests.Shared
{
    public class OrderItemTestHelper(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        public async Task<OrderItem> CreateTestOrderItem(int orderId, int productId, double price, int quantity = 1)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var newOrderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitaryPrice = price,
            };

            dbContext.OrderItens.Add(newOrderItem);
            await dbContext.SaveChangesAsync();

            return newOrderItem;
        }

        public async Task EnsureOrderExists(int orderId)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingOrder = await dbContext.Orders.FindAsync(orderId);
            if (existingOrder == null)
            {
                dbContext.Orders.Add(new Order { Id = orderId });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
