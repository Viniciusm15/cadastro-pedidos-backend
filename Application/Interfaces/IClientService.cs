using Domain.Models.Entities;
using Domain.Models.RequestModels;

namespace Application.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClients();
        Task<Client> GetClientById(int id);
        Task<Client> CreateClient(ClientRequestModel clientRequestModel);
        Task UpdateClient(int id, ClientRequestModel clientRequestModel);
        Task DeleteClient(int id);
    }
}
