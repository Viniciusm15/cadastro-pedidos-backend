using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemResponseModel>> GetOrderItemsByOrderId(int orderId);
        Task<OrderItemResponseModel> CreateOrderItem(OrderItemRequestModel orderItemRequestModel);
        Task UpdateOrderItem(int orderItemId, OrderItemRequestModel orderItemRequestModel);
        Task SyncOrderItems(int orderId, IEnumerable<OrderItemRequestModel> itemRequests);
        Task DeleteOrderItem(int id);
    }
}
