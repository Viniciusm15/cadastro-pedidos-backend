using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseModel>> GetAllCategories();
        Task<CategoryResponseModel> GetCategoryById(int id);
        Task<CategoryResponseModel> CreateCategory(CategoryRequestModel categoryRequestModel);
        Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel);
        Task DeleteCategory(int id);
    }
}
