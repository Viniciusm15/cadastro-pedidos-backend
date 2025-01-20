using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class OrderResponseModel : OrderBaseModel
    {
        public int OrderId { get; set; }

        public string StatusDescription => Status.ToString();
    }
}
