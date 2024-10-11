using Common.Models;
using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<PagedResult<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<Product?> GetProductByIdAsync(int id);
    }
}
