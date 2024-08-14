using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models.ResponseModels;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ILogger<OrderItemService> _logger;
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(ILogger<OrderItemService> logger, IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderItemResponseModel>> GetAllOrderItems()
        {
            _logger.LogInformation("Retrieving all categories");
            var orders = await _orderItemRepository.GetAllOrderItemsAsync();

            _logger.LogInformation("Retrieved {CategoryCount} categories", orders.Count());
            return orders.Select(orderItem => new OrderItemResponseModel
            {
                OrderId = orderItem.OrderId,
                Quantity = orderItem.Quantity,
                UnitaryPrice = orderItem.UnitaryPrice,
                Subtotal = orderItem.Subtotal,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.Name,
                OrderItemId = orderItem.OrderId
            });
        }
    }
}
