using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryResponseModel>> GetAllCategories(int pageNumber, int pageSize);
        Task<CategoryResponseModel> GetCategoryById(int id);
        Task<CategoryResponseModel> CreateCategory(CategoryRequestModel categoryRequestModel);
        Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel);
        Task DeleteCategory(int id);
    }
}
