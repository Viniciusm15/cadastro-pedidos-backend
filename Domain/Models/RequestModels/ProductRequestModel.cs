using Domain.Models.Base;

namespace Domain.Models.RequestModels
{
    public class ProductRequestModel : ProductBaseModel
    {
        public required ImageRequestModel Image { get; set; }  
    }
}
