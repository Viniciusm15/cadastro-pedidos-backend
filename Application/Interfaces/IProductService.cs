using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponseModel>> GetAllProducts(int pageNumber, int pageSize);
        Task<ProductResponseModel> GetProductById(int id);
        Task<ProductResponseModel> CreateProduct(ProductRequestModel productRequestModel);
        Task UpdateProduct(int id, ProductRequestModel productRequestModel);
        Task DeleteProduct(int id);
        Task<int> GetLowStockProductsCountAsync(int threshold = 10);
        Task<PagedResult<DashboardLowStockProductResponseModel>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10, int threshold = 10);
        Task<DashboardRestockResponseModel> RestockProductAsync(int productId, int restockQuantity);
    }
}
