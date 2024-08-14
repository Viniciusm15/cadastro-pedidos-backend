using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<ClientResponseModel>> GetAllClients();
        Task<ClientResponseModel> GetClientById(int id);
        Task<ClientResponseModel> CreateClient(ClientRequestModel clientRequestModel);
        Task UpdateClient(int id, ClientRequestModel clientRequestModel);
        Task DeleteClient(int id);
    }
}
