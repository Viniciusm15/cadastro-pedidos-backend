using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class CategoryResponseModel : CategoryBaseModel
    {
        public int CategoryId { get; set; }
        public int ProductCount { get; set; }
    }
}
