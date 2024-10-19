using Domain.Models.Entities;

namespace Domain.Interfaces
{
    public interface IImageRepository : IGenericRepository<Image>
    {
        Task<Image?> GetImageByIdAsync(int id);
    }
}
