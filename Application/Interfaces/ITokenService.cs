using Domain.Models.Entities;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user, int clientId);
    }
}
