using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IValidator<Category> _categoryValidator;

        public CategoryService(ICategoryRepository categoryRepository, IValidator<Category> categoryValidator)
        {
            _categoryRepository = categoryRepository;
            _categoryValidator = categoryValidator;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id) ?? throw new NotFoundException($"Category not found by ID: {id}");
        }

        public async Task<Category> CreateCategory(CategoryRequestModel categoryRequestModel)
        {
            var category = new Category
            {
                Name = categoryRequestModel.Name,
                Description = categoryRequestModel.Description
            };

            var validationResult = _categoryValidator.Validate(category);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _categoryRepository.CreateAsync(category);
            return category;
        }

        public async Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel)
        {
            var category = await GetCategoryById(id);

            category.Name = categoryRequestModel.Name;
            category.Description = categoryRequestModel.Description;

            var validationResult = _categoryValidator.Validate(category);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategory(int id)
        {
            var category = await GetCategoryById(id);
            await _categoryRepository.DeleteAsync(category);
        }
    }
}
