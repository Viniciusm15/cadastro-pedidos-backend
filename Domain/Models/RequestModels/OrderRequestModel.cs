using Domain.Models.Base;

namespace Domain.Models.RequestModels
{
    public class OrderRequestModel : OrderBaseModel
    {
        public IEnumerable<OrderItemRequestModel> OrderItems { get; set; }
    }
}
