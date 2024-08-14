using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client?> GetClientByIdAsync(int id);
    }
}
