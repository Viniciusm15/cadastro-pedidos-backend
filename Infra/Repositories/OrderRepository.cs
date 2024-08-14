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

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .ToListAsync();
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
