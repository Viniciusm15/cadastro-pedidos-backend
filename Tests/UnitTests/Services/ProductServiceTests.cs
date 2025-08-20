using Application.Interfaces;
using Application.Services;
using Common.Exceptions;
using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IValidator<Product>> _productValidatorMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProductService>>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _productValidatorMock = new Mock<IValidator<Product>>();
            _imageServiceMock = new Mock<IImageService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productService = new ProductService(
                _loggerMock.Object,
                _productRepositoryMock.Object,
                _productValidatorMock.Object,
                _imageServiceMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnPagedProducts()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var products = new List<Product>
            {
                new() { Id = 1, Name = "Product 1", Description = "Description 1" },
                new() { Id = 2, Name = "Product 2", Description = "Description 2" }
            };

            _productRepositoryMock
                .Setup(x => x.GetAllProductsAsync(pageNumber, pageSize))
                .ReturnsAsync(new PagedResult<Product>(products, 2));

            // Act
            var result = await _productService.GetAllProducts(pageNumber, pageSize);

            // Assert
            Assert.Equal(2, result.Items.Count());

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieved {products.Count} products on page {pageNumber}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task GetProductById_ShouldReturnProduct_WhenExists()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product", Description = "Test Description" };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductById(productId);

            // Assert
            Assert.Equal(productId, result.ProductId);

            _loggerMock.Verify(
               x => x.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product found by ID: {productId}")),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task GetProductById_ShouldThrowNotFoundException_WhenNotExists()
        {
            // Arrange
            var productId = 999;

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _productService.GetProductById(productId));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product not found by ID: {productId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task CreateProduct_ShouldCreateProduct_WhenValid()
        {
            // Arrange
            var requestModel = new ProductRequestModel
            {
                Name = "",
                Description = "Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "New Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x78, 0x90, 0xAB]
                }
            };

            var imageResult = new ImageResponseModel
            {
                ImageId = 1,
                Description = "New Image",
                ImageMimeType = "image/png",
                ImageData = [0x78, 0x90, 0xAB]
            };
            var createdProduct = new Product
            {
                Id = 1,
                Name = "New Product",
                Description = "Product Description",
            };

            _imageServiceMock
                .Setup(x => x.CreateImage(requestModel.Image, "Product", 0))
                .ReturnsAsync(imageResult);

            _productValidatorMock
                .Setup(x => x.Validate(It.IsAny<Product>()))
                .Returns(new ValidationResult());

            _productRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .Callback<Product>(p => p.Id = createdProduct.Id)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateProduct(requestModel);

            // Assert
            Assert.Equal(createdProduct.Id, result.ProductId);
            _imageServiceMock.Verify(x => x.UpdateImage(imageResult.ImageId, requestModel.Image, createdProduct.Id, "Product"), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product created with ID: {createdProduct.Id}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task CreateProduct_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var requestModel = new ProductRequestModel
            {
                Name = "",
                Description = "Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "Product Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x01, 0x02, 0x03]
                }
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _imageServiceMock
                .Setup(x => x.CreateImage(It.IsAny<ImageRequestModel>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new ImageResponseModel
                {
                    ImageId = 1,
                    Description = "Product Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x01, 0x02, 0x03]
                });

            _productValidatorMock
                .Setup(x => x.Validate(It.IsAny<Product>()))
                .Returns(new ValidationResult(validationErrors));

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.RollbackAsync())
                .Returns(Task.CompletedTask);

            // Act
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _productService.CreateProduct(requestModel));

            // Assert
            exception.ValidationErrors.Should().Contain("Name is required");

            _imageServiceMock.Verify(
                x => x.CreateImage(It.IsAny<ImageRequestModel>(), typeof(Product).Name, 0),
                Times.Once);

            _productRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<Product>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.CommitAsync(),
                Times.Never);

            _unitOfWorkMock.Verify(
                x => x.RollbackAsync(),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Product creation failed due to validation errors")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var requestModel = new ProductRequestModel
            {
                Name = "New Product",
                Description = "Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "New Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x78, 0x90, 0xAB]
                }
            };

            var exception = new Exception("Test exception");

            _imageServiceMock
                .Setup(x => x.CreateImage(It.IsAny<ImageRequestModel>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(exception);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _productService.CreateProduct(requestModel));

            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product creation failed. Rolling back transaction")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateProduct_WhenValid()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                Id = productId,
                ImageId = 1,
                Name = "New Product",
                Description = "Product Description",
            };

            var requestModel = new ProductRequestModel
            {
                Name = "Updated Product",
                Description = "Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "New Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x78, 0x90, 0xAB]
                }
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _productValidatorMock
                .Setup(x => x.Validate(It.IsAny<Product>()))
                .Returns(new ValidationResult());

            // Act
            await _productService.UpdateProduct(productId, requestModel);

            // Assert
            _productRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.Name == requestModel.Name)), Times.Once);
            _imageServiceMock.Verify(x => x.UpdateImage(existingProduct.ImageId, requestModel.Image, productId, "Product"), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product updated with ID: {productId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task UpdateProduct_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            var requestModel = new ProductRequestModel
            {
                Name = "Updated Product",
                Description = "Updated Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "Updated Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x78, 0x90, 0xAB]
                }
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _productService.UpdateProduct(productId, requestModel));

            _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var productId = 1;
            var requestModel = new ProductRequestModel
            {
                Name = "",
                Description = "Product Description",
                Price = 9.99,
                StockQuantity = 10,
                Image = new ImageRequestModel
                {
                    Description = "Image",
                    ImageMimeType = "image/png",
                    ImageData = [0x78, 0x90, 0xAB]
                }
            };

            var existingProduct = new Product
            {
                Id = productId,
                Name = "Original Name",
                Description = "Original Description",
                Price = 10,
                StockQuantity = 1
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _productValidatorMock
                .Setup(x => x.Validate(It.IsAny<Product>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _productService.UpdateProduct(productId, requestModel));

            exception.ValidationErrors.Should().Contain("Name is required");

            _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_ShouldNotUpdateImage_WhenImageNull()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                Id = productId,
                ImageId = 1,
                Name = "Existing Product",
                Description = "Existing Description",
                Price = 10.99,
                StockQuantity = 5
            };

            var requestModel = new ProductRequestModel
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 9.9,
                StockQuantity = 10,
                Image = null
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _productValidatorMock
                .Setup(x => x.Validate(It.IsAny<Product>()))
                .Returns(new ValidationResult());

            // Act
            await _productService.UpdateProduct(productId, requestModel);

            // Assert
            _imageServiceMock.Verify(
                x => x.UpdateImage(
                    It.IsAny<int>(),
                    It.IsAny<ImageRequestModel>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()
                ),
                Times.Never);

            _productRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Product>(p =>
                    p.Id == productId &&
                    p.Name == requestModel.Name &&
                    p.Description == requestModel.Description &&
                    p.ImageId == existingProduct.ImageId)),
                Times.Once);

            _imageServiceMock.Verify(
                x => x.CreateImage(It.IsAny<ImageRequestModel>(), It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteProduct_ShouldDeleteProductAndImage()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                ImageId = 1,
                Name = "New Product",
                Description = "Product Description",
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            await _productService.DeleteProduct(productId);

            // Assert
            _imageServiceMock.Verify(x => x.DeleteImage(product.ImageId), Times.Once);
            _productRepositoryMock.Verify(x => x.DeleteAsync(product), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Product deleted with ID: {productId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task DeleteProduct_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 1;

            _productRepositoryMock
               .Setup(x => x.GetProductByIdAsync(productId))
               .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _productService.DeleteProduct(productId));

            _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task GetLowStockProductsCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var threshold = 10;
            var expectedCount = 5;

            _productRepositoryMock
                .Setup(x => x.GetLowStockProductsCountAsync(threshold))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _productService.GetLowStockProductsCountAsync(threshold);

            // Assert
            result.Should().Be(expectedCount);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieving low stock products")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieved total low stock products")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetLowStockProductsCountAsync_ShouldUseDefaultThreshold()
        {
            // Arrange
            var defaultThreshold = 10;
            var expectedCount = 3;

            _productRepositoryMock
                .Setup(x => x.GetLowStockProductsCountAsync(defaultThreshold))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _productService.GetLowStockProductsCountAsync();

            // Assert
            result.Should().Be(expectedCount);
            _productRepositoryMock.Verify(x => x.GetLowStockProductsCountAsync(defaultThreshold), Times.Once);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnPagedLowStockProducts()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var threshold = 5;
            var products = new List<Product>
            {
                new() { Id = 1, Name = "Product 1", Description = "Description 1" },
                new() { Id = 2, Name = "Product 2", Description = "Description 2" }
            };

            var pagedProducts = new PagedResult<Product>(products, 2);

            _productRepositoryMock
                .Setup(x => x.GetLowStockProductsAsync(pageNumber, pageSize, threshold))
                .ReturnsAsync(pagedProducts);

            // Act
            var result = await _productService.GetLowStockProductsAsync(pageNumber, pageSize, threshold);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        $"Retrieving low stock products (threshold: {threshold}) for page {pageNumber} with size {pageSize}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        $"Retrieved {products.Count} low stock products on page {pageNumber}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldUseDefaultParameters()
        {
            // Arrange
            var defaultPageNumber = 1;
            var defaultPageSize = 10;
            var defaultThreshold = 10;

            _productRepositoryMock
                .Setup(x => x.GetLowStockProductsAsync(defaultPageNumber, defaultPageSize, defaultThreshold))
                .ReturnsAsync(new PagedResult<Product>([], 0));

            // Act
            await _productService.GetLowStockProductsAsync();

            // Assert
            _productRepositoryMock.Verify(
                x => x.GetLowStockProductsAsync(defaultPageNumber, defaultPageSize, defaultThreshold),
                Times.Once);
        }

        [Fact]
        public async Task RestockProductAsync_ShouldRestockProduct_WhenValid()
        {
            // Arrange
            var productId = 1;
            var restockQuantity = 5;
            var initialStock = 10;
            var product = new Product
            {
                Id = productId,
                StockQuantity = initialStock,
                Name = "Test Product",
                Description = "Test Description"
            };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            _productValidatorMock
                .Setup(x => x.Validate(product))
                .Returns(new ValidationResult());

            // Act
            var result = await _productService.RestockProductAsync(productId, restockQuantity);

            // Assert
            result.Should().NotBeNull();
            result.NewStockQuantity.Should().Be(initialStock + restockQuantity);
            result.Message.Should().Be("Stock replenished successfully");

            _productRepositoryMock.Verify(x => x.UpdateAsync(product), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        $"Starting product restock for ID {productId} with quantity {restockQuantity}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        $"Product restocked successfully. Product: {productId}, New stock: {initialStock + restockQuantity}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestockProductAsync_ShouldThrowNotFoundException_WhenProductNotFound()
        {
            // Arrange
            var productId = 999;
            var restockQuantity = 5;

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _productService.RestockProductAsync(productId, restockQuantity));

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        $"Product not found for restock: {productId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestockProductAsync_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var productId = 1;
            var restockQuantity = -5; // Invalid quantity
            var product = new Product
            {
                Id = productId,
                StockQuantity = 10,
                Name = "Test Product",
                Description = "Test Description"
            };

            var validationErrors = new List<ValidationFailure>
    {
        new("StockQuantity", "Stock quantity cannot be negative")
    };

            _productRepositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            _productValidatorMock
                .Setup(x => x.Validate(product))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _productService.RestockProductAsync(productId, restockQuantity));

            exception.ValidationErrors.Should().Contain("Stock quantity cannot be negative");

            // Verify error logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
                        "Product restock failed due to validation errors")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
