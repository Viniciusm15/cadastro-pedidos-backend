using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models.Entities;

namespace Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<OrderItem>> GetAllOrderItems()
        {
            return await _orderItemRepository.GetAllOrderItemsAsync();
        }
    }
}
