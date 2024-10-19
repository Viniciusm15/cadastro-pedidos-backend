using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products
               .WhereActive()
               .OrderBy(product => product.Name)
               .Include(product => product.Category)
               .Include(product => product.OrderItens)
               .Include(product => product.Image);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .WhereActive()
                .Include(product => product.Category)
                .Include(product => product.Image)
                .Include(product => product.OrderItens)
                .FirstOrDefaultAsync(product => product.Id == id);
        }
    }
}
