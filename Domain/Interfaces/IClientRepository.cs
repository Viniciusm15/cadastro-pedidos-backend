using Common.Models;
using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<PagedResult<Client>> GetAllClientsAsync(int pageNumber, int pageSize);
        Task<Client?> GetClientByIdAsync(int id);
        Task<int> GetActiveClientsCountAsync(int months);
        Task<int> GetNewClientsCountAsync(int month, int year);
        Task<int> GetTotalClientsCountAsync();
        Task<int> GetClientsCountUntilDateAsync(DateTime startDate, DateTime endDate);
    }
}
