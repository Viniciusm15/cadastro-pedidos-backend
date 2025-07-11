using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class ProductResponseModel : ProductBaseModel
    {
        public int ProductId { get; set; }
        public ImageResponseModel? Image { get; set; }
    }
}
