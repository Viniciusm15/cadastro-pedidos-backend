using Domain.Models.Base;

namespace Domain.Models.RequestModels
{
    public class OrderItemRequestModel : OrderItemBaseModel
    {
        public int? Id { get; set; }
    }
}
