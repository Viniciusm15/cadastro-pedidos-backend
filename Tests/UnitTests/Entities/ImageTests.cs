using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;

namespace Tests.UnitTests.Entities
{
    public class ImageTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Arrange & Act
            var image = new Image
            {
                ImageData = [0x01, 0x02, 0x03],
                ImageMimeType = "image/jpeg",
                EntityId = 1,
                EntityType = "Product"
            };

            // Assert
            image.IsActive.Should().BeTrue();
            image.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            image.DeletedAt.Should().BeNull();
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var imageData = new byte[] { 0x01, 0x02, 0x03 };
            var image = new Image
            {
                ImageData = imageData,
                ImageMimeType = "image/png",
                Description = "Sample image",
                EntityId = 1,
                EntityType = "Category"
            };

            // Act & Assert
            image.ImageData.Should().BeEquivalentTo(imageData);
            image.ImageMimeType.Should().Be("image/png");
            image.Description.Should().Be("Sample image");
            image.EntityId.Should().Be(1);
            image.EntityType.Should().Be("Category");
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var image = new Image
            {
                ImageData = [0x01],
                ImageMimeType = "image/jpeg",
                EntityId = 1,
                EntityType = "Product",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Assert
            image.DeletedAt.Should().NotBeNull();
            image.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new ImageValidator();
            var image = new Image
            {
                ImageData = [0x01],
                ImageMimeType = "image/jpeg",
                EntityId = 1,
                EntityType = "Product",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Act
            var result = image.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectEmptyDataOrInvalidMimeType()
        {
            // Arrange
            var validator = new ImageValidator();
            var image = new Image
            {
                ImageData = [],
                ImageMimeType = "invalid",
                EntityId = 0,
                EntityType = ""
            };

            // Act
            var result = validator.Validate(image);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ImageData");
            result.Errors.Should().Contain(e => e.PropertyName == "ImageMimeType");
        }
    }
}
