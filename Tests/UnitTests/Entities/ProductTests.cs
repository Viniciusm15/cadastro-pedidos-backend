using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;
using FluentValidation;

namespace Tests.UnitTests.Entities
{
    public class ProductTests
    {
        [Fact]
        public void Constructor_ShouldInitializeOrderItensAsEmptyList()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Smartphone",
                Description = "Latest model",
                Price = 999.99,
                StockQuantity = 10
            };

            // Assert
            product.OrderItens.Should().NotBeNull();
            product.OrderItens.Should().BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop",
                Description = "High performance",
                Price = 1500.00,
                StockQuantity = 5,
                CategoryId = 1,
                ImageId = 1
            };

            // Act & Assert
            product.Name.Should().Be("Laptop");
            product.Description.Should().Be("High performance");
            product.Price.Should().Be(1500.00);
            product.StockQuantity.Should().Be(5);
            product.CategoryId.Should().Be(1);
            product.ImageId.Should().Be(1);
            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var product = new Product
            {
                Name = "Tablet",
                Description = "Portable device",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Assert
            product.DeletedAt.Should().NotBeNull();
            product.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new ProductValidator();
            var product = new Product
            {
                Name = "Valid Name",
                Description = "Valid Description",
                Price = 999.99,
                StockQuantity = 10,
                CategoryId = 1
            };

            // Act
            var result = product.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectInvalidData()
        {
            // Arrange
            var validator = new ProductValidator();
            var product = new Product
            {
                Name = "", 
                Description = "", 
                Price = -100,
                StockQuantity = -1,
                CategoryId = 0,
                ImageId = 0 
            };

            // Act
            var result = validator.Validate(product);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
            result.Errors.Should().Contain(e => e.PropertyName == "Description");
            result.Errors.Should().Contain(e => e.PropertyName == "Price");
            result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
            result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
        }

        [Fact]
        public void CategoryAndImage_Relationships_ShouldBeSynchronized()
        {
            // Arrange
            var category = new Category {
                Id = 2, 
                Name = "Electronics", 
                Description = "Devices and gadgets" 
            };
            var image = new Image
            {
                Id = 2,
                ImageData = [0x01, 0x02, 0x03],
                ImageMimeType = "image/jpeg",
                EntityId = 1,
                EntityType = "Product"
            };
            var product = new Product
            {
                Name = "Product Name",
                Description = "Product Description",
                Category = category,
                Image = image
            };

            // Act & Assert
            product.Category.Id.Should().Be(2);
            product.Image.Id.Should().Be(2);
        }
    }
}
