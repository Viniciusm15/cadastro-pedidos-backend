using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            var query = _context.Categories
                .WhereActive()
                .OrderBy(category => category.Name)
                .Include(category => category.Products);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Category>(items, totalCount);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .WhereActive()
                .Include(category => category.Products)
                .FirstOrDefaultAsync(category => category.Id == id);
        }
    }
}
