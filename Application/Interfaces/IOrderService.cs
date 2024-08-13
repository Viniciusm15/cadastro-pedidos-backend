using Domain.Models.Entities;
using Domain.Models.RequestModels;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrders();
        Task<Order> GetOrderById(int id);
        Task<Order> CreateOrder(OrderRequestModel orderRequestModel);
        Task UpdateOrder(int id, OrderRequestModel orderRequestModel);
        Task DeleteOrder(int id);
    }
}
