using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class CategoryControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_ReturnsPagedCategories()
        {
            // Act
            var response = await _client.GetAsync("/api/category?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<CategoryResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewCategory()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                Name = "Test Category",
                Description = "Test Description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/category", newCategory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdCategory = await DeserializeResponse<CategoryResponseModel>(response);
            createdCategory.Name.Should().Be(newCategory.Name);
            createdCategory.Description.Should().Be(newCategory.Description);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidCategory = new CategoryRequestModel
            {
                Name = "",
                Description = "Test Description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/category", invalidCategory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedCategory()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                Name = "Test GetById",
                Description = "Test Description"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/category", newCategory);
            var createdCategory = await DeserializeResponse<CategoryResponseModel>(createResponse);

            // Act
            var response = await _client.GetAsync($"/api/category/{createdCategory.CategoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var category = await DeserializeResponse<CategoryResponseModel>(response);
            category.CategoryId.Should().Be(createdCategory.CategoryId);
            category.Name.Should().Be(newCategory.Name);
            category.Description.Should().Be(newCategory.Description);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var response = await _client.GetAsync($"/api/category/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingCategory()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                Name = "Test Put",
                Description = "Test Description"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/category", newCategory);
            var createdCategory = await DeserializeResponse<CategoryResponseModel>(createResponse);

            var updatedCategory = new CategoryRequestModel
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/category/{createdCategory.CategoryId}", updatedCategory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/category/{createdCategory.CategoryId}");
            var category = await DeserializeResponse<CategoryResponseModel>(getResponse);
            category.Name.Should().Be(updatedCategory.Name);
            category.Description.Should().Be(updatedCategory.Description);
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var updatedCategory = new CategoryRequestModel
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/category/{nonExistentId}", updatedCategory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                Name = "Test Put Invalid",
                Description = "Test Description"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/category", newCategory);
            var createdCategory = await DeserializeResponse<CategoryResponseModel>(createResponse);

            var invalidCategory = new CategoryRequestModel
            {
                Name = "", // Nome inválido
                Description = "Updated Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/category/{createdCategory.CategoryId}", invalidCategory);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesCategory()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                Name = "Test Delete",
                Description = "Test Description"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/category", newCategory);
            var createdCategory = await DeserializeResponse<CategoryResponseModel>(createResponse);

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
            var response = await _client.DeleteAsync($"/api/category/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
