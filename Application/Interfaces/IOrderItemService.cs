using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemResponseModel>> GetAllOrderItems();
    }
}
