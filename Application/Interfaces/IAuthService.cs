using Domain.Models.RequestModels;
using Domain.Models.RequestModels.AuthRequestModels;
using Domain.Models.ResponseModels.AuthResponseModels;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequest);
        Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel registerRequest);
        Task<UserProfileResponseModel> GetUserProfileAsync(string userId);
        Task UpdateUserProfileAsync(string currentUserId, int? targetClientId, ClientRequestModel clientRequestModel);
        Task DeleteUserProfileAsync(string currentUserId, int? targetClientId = null);
    }
}
