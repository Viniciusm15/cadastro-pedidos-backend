using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class ClientResponseModel : ClientBaseModel
    {
        public int ClientId { get; set; }
        public required string ApplicationUserId { get; set; }
        public ICollection<OrderResponseModel> PurchaseHistory { get; set; } = [];
    }
}
