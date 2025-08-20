using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Client>> GetAllClientsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Clients
                .WhereActive()
                .OrderBy(client => client.Name)
                .Include(client => client.Orders.Where(client => client.IsActive));

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Client>(items, totalCount);
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients
                .WhereActive()
                .Include(client => client.Orders.Where(client => client.IsActive))
                .FirstOrDefaultAsync(client => client.Id == id);
        }

        public async Task<int> GetActiveClientsCountAsync(int months)
        {
            var dateThreshold = DateTime.Now.AddMonths(-months);
            return await _context.Clients
                .WhereActive()
                .Where(c => c.Orders.Any(o => o.OrderDate >= dateThreshold))
                .CountAsync();
        }

        public async Task<int> GetNewClientsCountAsync(int month, int year)
        {
            return await _context.Clients
                .WhereActive()
                .Where(c => c.CreatedAt.Month == month && c.CreatedAt.Year == year)
                .CountAsync();
        }

        public async Task<int> GetTotalClientsCountAsync()
        {
            return await _context.Clients
                .WhereActive()
                .CountAsync();
        }

        public async Task<int> GetClientsCountUntilDateAsync(DateTime endDate)
        {
            return await _context.Clients
                .WhereActive()
                .Where(c => c.CreatedAt <= endDate)
                .CountAsync();
        }
    }
}
