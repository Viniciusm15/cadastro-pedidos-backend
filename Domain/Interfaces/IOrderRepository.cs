using Common.Models;
using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<Order?> GetOrderByIdAsync(int id);
    }
}
