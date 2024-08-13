using Domain.Models.Entities;
using Domain.Models.RequestModels;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProductById(int id);
        Task<Product> CreateProduct(ProductRequestModel productRequestModel);
        Task UpdateProduct(int id, ProductRequestModel productRequestModel);
        Task DeleteProduct(int id);
    }
}
