using Common.Models;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class CategoryControllerTests : IntegrationTestBase
    {
        private readonly CategoryTestHelper _categoryHelper;

        public CategoryControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _categoryHelper = new CategoryTestHelper(_client);
        }

        [Fact]
        public async Task GetAll_ReturnsPagedCategories()
        {
            // Arrange
            await _categoryHelper.CreateTestCategory();

            // Act
            var getResponse = await _client.GetAsync("/api/category?pageNumber=1&pageSize=10");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<CategoryResponseModel>>(getResponse);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewCategory()
        {
            // Arrange
            var categoryRequestModel = _categoryHelper.CreateCategoryRequestModel();

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/category", categoryRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse.Headers.Location.Should().NotBeNull();

            var categoryResponseModel = await DeserializeResponse<CategoryResponseModel>(postResponse);
            categoryResponseModel.CategoryId.Should().BeGreaterThan(0);
            categoryResponseModel.Name.Should().Be(categoryRequestModel.Name);
            categoryResponseModel.Description.Should().Be(categoryRequestModel.Description);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidCategoryRequestModel = _categoryHelper.CreateCategoryRequestModel(name: "");

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/category", invalidCategoryRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedCategory()
        {
            // Arrange
            var createdCategory = await _categoryHelper.CreateTestCategory();

            // Act
            var getResponse = await _client.GetAsync($"/api/category/{createdCategory.CategoryId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var categoryResponseModel = await DeserializeResponse<CategoryResponseModel>(getResponse);
            categoryResponseModel.CategoryId.Should().Be(createdCategory.CategoryId);
            categoryResponseModel.Name.Should().Be(createdCategory.Name);
            categoryResponseModel.Description.Should().Be(createdCategory.Description);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var getResponse = await _client.GetAsync($"/api/category/{nonExistentId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingCategory()
        {
            // Arrange
            var createdCategory = await _categoryHelper.CreateTestCategory();
            var updatedCategoryRequestModel = _categoryHelper.CreateCategoryRequestModel(
                name: "Updated Name", description: "Updated Description");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/category/{createdCategory.CategoryId}", updatedCategoryRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/category/{createdCategory.CategoryId}");
            var categoryResponseModel = await DeserializeResponse<CategoryResponseModel>(getResponse);
            categoryResponseModel.Name.Should().Be(updatedCategoryRequestModel.Name);
            categoryResponseModel.Description.Should().Be(updatedCategoryRequestModel.Description);
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var updatedCategoryRequestModel = _categoryHelper.CreateCategoryRequestModel(
                name: "Updated Name", description: "Updated Description");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/category/{nonExistentId}", updatedCategoryRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createdCategory = await _categoryHelper.CreateTestCategory();
            var invalidCategoryRequestModel = _categoryHelper.CreateCategoryRequestModel(name: "");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/category/{createdCategory.CategoryId}", invalidCategoryRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesCategory()
        {
            // Arrange
            var createdCategory = await _categoryHelper.CreateTestCategory();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/category/{createdCategory.CategoryId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/category/{createdCategory.CategoryId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/category/{nonExistentId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
