using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class OrderItemResponseModel : OrderItemBaseModel
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int OrderId { get; set; }
    }
}
