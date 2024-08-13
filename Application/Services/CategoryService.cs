using Application.Interfaces;
using Common.Exceptions;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Category> _categoryValidator;

        public CategoryService(ApplicationDbContext context, IValidator<Category> categoryValidator)
        {
            _context = context;
            _categoryValidator = categoryValidator;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _context.Categories
                .WhereActive()
                .OrderBy(category => category.Name)
                .Include(category => category.Products)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .WhereActive()
                .Include(category => category.Products)
                .FirstOrDefaultAsync(category => category.Id == id);

            return category ?? throw new NotFoundException($"Category not found by ID: {id}");
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

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task UpdateCategory(int id, CategoryRequestModel categoryRequestModel)
        {
            var category = await _context.Categories.FindAsync(id) ?? throw new NotFoundException($"Category not found by ID: {id}");

            category.Name = categoryRequestModel.Name;
            category.Description = categoryRequestModel.Description;

            var validationResult = _categoryValidator.Validate(category);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id) ?? throw new NotFoundException($"Category not found by ID: {id}");

            category.IsActive = false;
            category.DeletedAt = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
