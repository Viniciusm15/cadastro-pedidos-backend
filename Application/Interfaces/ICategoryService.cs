using Domain.Models.Entities;
using Domain.Models.RequestModels;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category> GetCategoryById(int id);
        Task<Category> CreateCategory(CategoryRequestModel categoryRequestModel);
        Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel);
        Task DeleteCategory(int id);
    }
}
