using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderItens
                .Where(orderItem => orderItem.OrderId == orderId)
                .WhereActive()
                .Include(orderItem => orderItem.Order)
                .Include(orderItem => orderItem.Product)
                .ToListAsync();
        }
    }
}
