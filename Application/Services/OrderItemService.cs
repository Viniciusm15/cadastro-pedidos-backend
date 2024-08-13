using Application.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ApplicationDbContext _context;

        public OrderItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllOrderItems()
        {
            var orderItems = await _context.OrderItens
                .Include(orderItem => orderItem.Order)
                .Include(orderItem => orderItem.Product)
                .ToListAsync();

            return orderItems;
        }
    }
}
