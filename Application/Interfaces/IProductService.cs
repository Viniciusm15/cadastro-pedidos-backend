using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseModel>> GetProducts();
        Task<ProductResponseModel> GetProductById(int id);
        Task<ProductResponseModel> CreateProduct(ProductRequestModel productRequestModel);
        Task UpdateProduct(int id, ProductRequestModel productRequestModel);
        Task DeleteProduct(int id);
    }
}
