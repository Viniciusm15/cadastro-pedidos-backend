using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class OrderItemResponseModel : OrderItemBaseModel
    {
        public int OrderItemId { get; set; }
        public required string ProductName { get; set; }
    }
}
