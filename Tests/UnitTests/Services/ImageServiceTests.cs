using Application.Services;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class ImageServiceTests
    {
        private readonly Mock<ILogger<ImageService>> _loggerMock;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IValidator<Image>> _imageValidatorMock;
        private readonly ImageService _imageService;

        public ImageServiceTests()
        {
            _loggerMock = new Mock<ILogger<ImageService>>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _imageValidatorMock = new Mock<IValidator<Image>>();

            _imageService = new ImageService(
                _loggerMock.Object,
                _imageRepositoryMock.Object,
                _imageValidatorMock.Object);
        }

        [Fact]
        public async Task GetImageById_ShouldReturnImage_WhenImageExists()
        {
            // Arrange
            var imageId = 1;
            var expectedImage = new Image
            {
                Id = imageId,
                Description = "Test Image",
                ImageMimeType = "image/jpeg",
                ImageData = [0x12, 0x34, 0x56]
            };

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.GetImageById(imageId);

            // Assert
            result.Should().NotBeNull();
            result.ImageId.Should().Be(imageId);
            result.Description.Should().Be(expectedImage.Description);
            result.ImageMimeType.Should().Be(expectedImage.ImageMimeType);
            result.ImageData.Should().Equal(expectedImage.ImageData);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Starting image search with ID {imageId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetImageById_ShouldThrowNotFoundException_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _imageService.GetImageById(imageId));

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Image not found by ID: {imageId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateImage_ShouldReturnCreatedImage_WhenValidationPasses()
        {
            // Arrange
            var requestModel = new ImageRequestModel
            {
                Description = "New Image",
                ImageMimeType = "image/png",
                ImageData = [0x78, 0x90, 0xAB]
            };

            var entityType = "Product";
            var entityId = 1;

            var createdImage = new Image
            {
                Id = 1,
                Description = requestModel.Description,
                ImageMimeType = requestModel.ImageMimeType,
                ImageData = requestModel.ImageData,
                EntityType = entityType,
                EntityId = entityId
            };

            _imageValidatorMock
                .Setup(x => x.Validate(It.IsAny<Image>()))
                .Returns(new ValidationResult());

            _imageRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Image>()))
                .Callback<Image>(i => i.Id = createdImage.Id)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _imageService.CreateImage(requestModel, entityType, entityId);

            // Assert
            result.Should().NotBeNull();
            result.ImageId.Should().Be(createdImage.Id);
            result.Description.Should().Be(requestModel.Description);
            result.ImageMimeType.Should().Be(requestModel.ImageMimeType);
            result.ImageData.Should().Equal(requestModel.ImageData);

            _imageRepositoryMock.Verify(x => x.CreateAsync(It.Is<Image>(i =>
                i.Description == requestModel.Description &&
                i.ImageMimeType == requestModel.ImageMimeType &&
                i.ImageData == requestModel.ImageData &&
                i.EntityType == entityType &&
                i.EntityId == entityId)),
            Times.Once);
        }

        [Fact]
        public async Task CreateImage_ShouldThrowValidationException_WhenImageDataIsEmpty()
        {
            // Arrange
            var requestModel = new ImageRequestModel
            {
                Description = "Invalid Image",
                ImageMimeType = "image/jpeg",
                ImageData = []
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("ImageData", "Image data cannot be empty")
            };

            _imageValidatorMock
                .Setup(x => x.Validate(It.IsAny<Image>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _imageService.CreateImage(requestModel, "Product", 1));

            exception.ValidationErrors.Should().Contain("Image data cannot be empty");

            _imageRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task UpdateImage_ShouldUpdateImage_WhenValidationPasses()
        {
            // Arrange
            var imageId = 1;
            var entityId = 2;
            var entityType = "Category";

            var requestModel = new ImageRequestModel
            {
                Description = "Updated Image",
                ImageMimeType = "image/gif",
                ImageData = [0xCD, 0xEF, 0x01]
            };

            var existingImage = new Image
            {
                Id = imageId,
                Description = "Original Image",
                ImageMimeType = "image/jpeg",
                ImageData = [0x12, 0x34],
                EntityType = "Product",
                EntityId = 1
            };

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(existingImage);

            _imageValidatorMock
                .Setup(x => x.Validate(It.IsAny<Image>()))
                .Returns(new ValidationResult());

            // Act
            await _imageService.UpdateImage(imageId, requestModel, entityId, entityType);

            // Assert
            _imageRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Image>(i =>
                i.Id == imageId &&
                i.Description == requestModel.Description &&
                i.ImageMimeType == requestModel.ImageMimeType &&
                i.ImageData == requestModel.ImageData &&
                i.EntityType == entityType &&
                i.EntityId == entityId)),
            Times.Once);
        }

        [Fact]
        public async Task UpdateImage_ShouldThrowNotFoundException_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;
            var requestModel = new ImageRequestModel
            {
                Description = "Nonexistent Image",
                ImageMimeType = "image/png",
                ImageData = [0x12]
            };

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _imageService.UpdateImage(imageId, requestModel, 1, "Product"));

            _imageRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task UpdateImage_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var imageId = 999;
            var requestModel = new ImageRequestModel
            {
                Description = "Nonexistent Image",
                ImageMimeType = "invalid/type", // Invalid ImageMimeType
                ImageData = [0x12]
            };

            var existingImage = new Image
            {
                Id = imageId,
                Description = "Original Image",
                ImageMimeType = "image/jpeg",
                ImageData = new byte[1024],
                EntityType = "Product",
                EntityId = 1
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("ImageMimeType", "MIME type is required.")
            };

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(existingImage);

            _imageValidatorMock
                .Setup(x => x.Validate(It.IsAny<Image>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _imageService.UpdateImage(imageId, requestModel, 1, "Product"));

            exception.ValidationErrors.Should().Contain("MIME type is required.");

            _imageRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Image>()), Times.Never);
        }

        [Fact]
        public async Task DeleteImage_ShouldDeleteImage_WhenImageExists()
        {
            // Arrange
            var imageId = 1;
            var existingImage = new Image
            {
                Id = imageId,
                Description = "Image to Delete",
                ImageMimeType = "image/jpeg",
                ImageData = [0x12]
            };

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync(existingImage);

            // Act
            await _imageService.DeleteImage(imageId);

            // Assert
            _imageRepositoryMock.Verify(x => x.DeleteAsync(existingImage), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting image with ID: {imageId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteImage_ShouldThrowNotFoundException_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = 999;

            _imageRepositoryMock
                .Setup(x => x.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _imageService.DeleteImage(imageId));

            _imageRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Image>()), Times.Never);
        }
    }
}
