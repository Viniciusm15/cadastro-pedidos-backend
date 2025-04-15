using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var query = _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItens);

            return await query.ToListAsync();
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItens);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .WhereActive()
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .FirstOrDefaultAsync(order => order.Id == id);
        }
    }
}
