using Common.Models;
using Domain.Enums;
using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<double> GetTotalOrderSalesAsync();
        Task<double> GetMonthlyOrderSalesAsync(int month, int year);
        Task<int> GetOrdersCountByStatusAsync(params OrderStatus[] status);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PagedResult<Order>> GetPendingOrdersAsync(int pageNumber, int pageSize);
    }
}
