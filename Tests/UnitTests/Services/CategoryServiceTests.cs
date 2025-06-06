using Application.Services;
using Common.Exceptions;
using Common.Models;
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
    public class CategoryServiceTests
    {
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IValidator<Category>> _categoryValidatorMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _loggerMock = new Mock<ILogger<CategoryService>>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _categoryValidatorMock = new Mock<IValidator<Category>>();

            _categoryService = new CategoryService(
                _loggerMock.Object,
                _categoryRepositoryMock.Object,
                _categoryValidatorMock.Object);
        }

        [Fact]
        public async Task GetAllCategories_ShouldReturnPagedResult_WhenCategoriesExist()
        {
            //// Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var categories = new List<Category> {
                new() { Id = 1, Name = "Category 1", Description = "Desc 1", Products = [] },
                new() { Id = 2, Name = "Category 2", Description = "Desc 2", Products = [] }
            };

            var pagedResult = new PagedResult<Category>(categories, 2);

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoriesAsync(pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _categoryService.GetAllCategories(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.First().Name.Should().Be("Category 1");
            result.Items.Last().Name.Should().Be("Category 2");

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieving categories for page {pageNumber} with size {pageSize}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllCategories_ShouldReturnEmptyPagedResult_WhenNoCategoriesExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var emptyCategories = new List<Category>();
            var pagedResult = new PagedResult<Category>(emptyCategories, 0);

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoriesAsync(pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _categoryService.GetAllCategories(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Description = "Test Description",
                Products = []
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryById(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(categoryId);
            result.Name.Should().Be(category.Name);
            result.Description.Should().Be(category.Description);
            result.ProductCount.Should().Be(0);
        }

        [Fact]
        public async Task GetCategoryById_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _categoryService.GetCategoryById(categoryId));

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Category not found by ID: {categoryId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnCreatedCategory_WhenValidationPasses()
        {
            // Arrange
            var requestModel = new CategoryRequestModel
            {
                Name = "New Category",
                Description = "New Description"
            };

            var createdCategory = new Category
            {
                Id = 1,
                Name = requestModel.Name,
                Description = requestModel.Description,
                Products = []
            };

            _categoryValidatorMock
                .Setup(x => x.Validate(It.IsAny<Category>()))
                .Returns(new ValidationResult());

            _categoryRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .Callback<Category>(c => c.Id = createdCategory.Id)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.CreateCategory(requestModel);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(createdCategory.Id);
            result.Name.Should().Be(requestModel.Name);
            result.Description.Should().Be(requestModel.Description);
            result.ProductCount.Should().Be(0);

            _categoryRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task CreateCategory_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var requestModel = new CategoryRequestModel
            {
                Name = "", // Invalid name
                Description = "New Description"
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _categoryValidatorMock
                .Setup(x => x.Validate(It.IsAny<Category>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _categoryService.CreateCategory(requestModel));

            exception.ValidationErrors.Should().Contain("Name is required");

            _categoryRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Never);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Category creation failed due to validation errors")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_ShouldUpdateCategory_WhenValidationPasses()
        {
            // Arrange
            var categoryId = 1;
            var requestModel = new CategoryRequestModel
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            var existingCategory = new Category
            {
                Id = categoryId,
                Name = "Original Category",
                Description = "Original Description",
                Products = []
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _categoryValidatorMock
                .Setup(x => x.Validate(It.IsAny<Category>()))
                .Returns(new ValidationResult());

            // Act
            await _categoryService.UpdateCategory(categoryId, requestModel);

            // Assert
            _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Category>(c =>
                c.Id == categoryId &&
                c.Name == requestModel.Name &&
                c.Description == requestModel.Description)),
            Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;
            var requestModel = new CategoryRequestModel
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _categoryService.UpdateCategory(categoryId, requestModel));

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var categoryId = 1;
            var requestModel = new CategoryRequestModel
            {
                Name = "", // Invalid name
                Description = "Updated Description"
            };

            var existingCategory = new Category
            {
                Id = categoryId,
                Name = "Original Category",
                Description = "Original Description",
                Products = []
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _categoryValidatorMock
                .Setup(x => x.Validate(It.IsAny<Category>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _categoryService.UpdateCategory(categoryId, requestModel));

            exception.ValidationErrors.Should().Contain("Name is required");

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCategory_ShouldDeleteCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var existingCategory = new Category
            {
                Id = categoryId,
                Name = "Category to Delete",
                Description = "Description",
                Products = []
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            // Act
            await _categoryService.DeleteCategory(categoryId);

            // Assert
            _categoryRepositoryMock.Verify(x => x.DeleteAsync(existingCategory), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting category with ID: {categoryId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _categoryService.DeleteCategory(categoryId));

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }
    }
}
