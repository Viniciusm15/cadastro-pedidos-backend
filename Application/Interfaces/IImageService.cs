using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageResponseModel> GetImageById(int id);
        Task<ImageResponseModel> CreateImage(ImageRequestModel image);
        Task UpdateImage(int id, ImageRequestModel image);
        Task DeleteImage(int id);
    }
}
