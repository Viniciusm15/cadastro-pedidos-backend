using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IImageRepository _imageRepository;
        private readonly IValidator<Image> _imageValidator;

        public ImageService(ILogger<ImageService> logger, IImageRepository imageRepository, IValidator<Image> imageValidator)
        {
            _logger = logger;
            _imageRepository = imageRepository;
            _imageValidator = imageValidator;
        }

        public async Task<ImageResponseModel> GetImageById(int id)
        {
            _logger.LogInformation("Starting image search with ID {Id}", id);
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                _logger.LogError("Image not found by ID: {Id}", id);
                throw new NotFoundException($"Image not found by ID: {id}");
            }

            _logger.LogInformation("Image found by ID: {ImageId}", image.Id);
            return new ImageResponseModel
            {
                ImageId = image.Id,
                Description = image.Description,
                ImageMimeType = image.ImageMimeType,
                ImageData = image.ImageData
            };
        }

        public async Task<ImageResponseModel> CreateImage(ImageRequestModel imageRequestModel)
        {
            _logger.LogInformation("Starting image creation with request data: {ImageRequest}", imageRequestModel);

            var image = new Image
            {
                Description = imageRequestModel.Description,
                ImageMimeType = imageRequestModel.ImageMimeType,
                ImageData = imageRequestModel.ImageData
            };

            var validationResult = _imageValidator.Validate(image);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Image creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _imageRepository.CreateAsync(image);

            _logger.LogInformation("Image created with ID: {ImageId}", image.Id);
            return new ImageResponseModel
            {
                ImageId = image.Id,
                Description = image.Description,
                ImageMimeType = image.ImageMimeType,
                ImageData = image.ImageData
            };
        }

        public async Task UpdateImage(int id, ImageRequestModel imageRequestModel)
        {
            _logger.LogInformation("Starting image update with request data: {ImageRequest}", imageRequestModel);

            _logger.LogInformation("Starting image search with ID {Id}", id);
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                _logger.LogError("Image not found by ID: {Id}", id);
                throw new NotFoundException($"Image not found by ID: {id}");
            }

            _logger.LogInformation("Image found by ID: {ImageId}", image.Id);

            image.Description = imageRequestModel.Description;
            image.ImageMimeType = imageRequestModel.ImageMimeType;
            image.ImageData = imageRequestModel.ImageData;

            var validationResult = _imageValidator.Validate(image);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Image update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _imageRepository.UpdateAsync(image);
            _logger.LogInformation("Image updated with ID: {ImageId}", id);
        }

        public async Task DeleteImage(int id)
        {
            _logger.LogInformation("Deleting image with ID: {Id}", id);
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                _logger.LogError("Image not found by ID: {Id}", id);
                throw new NotFoundException($"Image not found by ID: {id}");
            }

            await _imageRepository.DeleteAsync(image);
            _logger.LogInformation("Image deleted with ID: {ProductId}", id);
        }
    }
}
