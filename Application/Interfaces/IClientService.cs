using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IClientService
    {
        Task<PagedResult<ClientResponseModel>> GetAllClients(int pageNumber, int pageSize);
        Task<ClientResponseModel> GetClientById(int id);
        Task<ClientResponseModel> GetClientByApplicationUserIdAsync(string applicationUserId);
        Task<ClientResponseModel> CreateClient(ClientRequestModel clientRequestModel, string userId);
        Task UpdateClient(int id, ClientRequestModel clientRequestModel, bool reactivateIfInactive = false);
        Task DeleteClient(int id);
        Task<int> GetActiveClientsCountAsync(int months = 6);
        Task<int> GetNewClientsThisMonthAsync();
        Task<DashboardClientSummaryResponseModel> GetClientDataAsync();
    }
}
