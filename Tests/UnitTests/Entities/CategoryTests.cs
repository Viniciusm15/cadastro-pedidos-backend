using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;

namespace Tests.UnitTests.Entities
{
    public class CategoryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProductsAsEmptyList()
        {
            // Arrange & Act
            var category = new Category
            {
                Name = "Electronics",
                Description = "Devices and gadgets"
            };

            // Assert
            category.Products.Should().NotBeNull();
            category.Products.Should().BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics",
                Description = "Devices and gadgets"
            };

            // Act & Assert
            category.Name.Should().Be("Electronics");
            category.Description.Should().Be("Devices and gadgets");
            category.IsActive.Should().BeTrue();
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics",
                Description = "Devices and gadgets",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Assert
            category.DeletedAt.Should().NotBeNull();
            category.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new CategoryValidator();
            var category = new Category
            {
                Name = "Valid Name",
                Description = "Valid Description"
            };

            // Act
            var result = category.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectNullNameOrDescription()
        {
            // Arrange
            var validator = new CategoryValidator();
            var category = new Category { Name = null!, Description = null! };

            // Act
            var result = validator.Validate(category);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
            result.Errors.Should().Contain(e => e.PropertyName == "Description");
        }
    }
}
