using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageResponseModel> GetImageById(int id);
        Task<ImageResponseModel> CreateImage(ImageRequestModel image, string entityType, int entityId);
        Task UpdateImage(int id, ImageRequestModel imageRequestModel, int entityId, string entityType);
        Task DeleteImage(int id);
    }
}
