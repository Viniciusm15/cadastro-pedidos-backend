using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseModel>> GetAllOrders();
        Task<OrderResponseModel> GetOrderById(int id);
        Task<OrderResponseModel> CreateOrder(OrderRequestModel orderRequestModel);
        Task UpdateOrder(int id, OrderRequestModel orderRequestModel);
        Task DeleteOrder(int id);
    }
}
