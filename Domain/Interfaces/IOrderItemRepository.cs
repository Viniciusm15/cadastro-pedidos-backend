using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetAllOrderItemsAsync();
    }
}
