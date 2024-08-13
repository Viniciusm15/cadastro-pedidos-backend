using Domain.Models.Entities;

namespace Application.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItem>> GetAllOrderItems();
    }
}
