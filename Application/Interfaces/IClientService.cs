using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IClientService
    {
        Task<PagedResult<ClientResponseModel>> GetAllClients(int pageNumber, int pageSize);
        Task<ClientResponseModel> GetClientById(int id);
        Task<ClientResponseModel> CreateClient(ClientRequestModel clientRequestModel);
        Task UpdateClient(int id, ClientRequestModel clientRequestModel);
        Task DeleteClient(int id);
    }
}
