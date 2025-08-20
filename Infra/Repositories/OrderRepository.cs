using Common.Models;
using Domain.Enums;
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
                .Include(order => order.OrderItems);

            return await query.ToListAsync();
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItems);

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
                .Include(order => order.OrderItems)
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<double> GetTotalOrderSalesAsync()
        {
            return await _context.Orders
                .WhereActive()
                .SumAsync(o => o.TotalValue);
        }

        public async Task<double> GetMonthlyOrderSalesAsync(int month, int year)
        {
            return await _context.Orders
                .WhereActive()
                .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year)
                .SumAsync(o => o.TotalValue);
        }

        public async Task<int> GetOrdersCountByStatusAsync(params OrderStatus[] status)
        {
            return await _context.Orders
                .WhereActive()
                .Where(o => status.Contains(o.Status))
                .CountAsync();
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .WhereActive()
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetPendingOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .WhereActive()
                .Include(o => o.Client)
                .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount);
        }
    }
}
