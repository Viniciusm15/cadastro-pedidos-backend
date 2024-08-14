﻿using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllOrderItemsAsync()
        {
            return await _context.OrderItens
                .WhereActive()
                .Include(orderItem => orderItem.Order)
                .Include(orderItem => orderItem.Product)
                .ToListAsync();
        }
    }
}
