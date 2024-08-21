using Common.Models;
using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<PagedResult<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<Category?> GetCategoryByIdAsync(int id);
    }
}
