using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class ClientControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_ReturnsPagedClients()
        {
            // Act
            var response = await _client.GetAsync("/api/client?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<ClientResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewClient()
        {
            // Arrange
            var newClient = new ClientRequestModel
            {
                Name = "Test Client",
                Email = "test@example.com",
                Telephone = "1234567890"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/client", newClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdClient = await DeserializeResponse<ClientResponseModel>(response);
            createdClient.Name.Should().Be(newClient.Name);
            createdClient.Email.Should().Be(newClient.Email);
            createdClient.Telephone.Should().Be(newClient.Telephone);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidClient = new ClientRequestModel
            {
                Name = "",
                Email = "invalid-email",
                Telephone = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/client", invalidClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedClient()
        {
            // Arrange
            var newClient = new ClientRequestModel
            {
                Name = "Test GetById",
                Email = "getbyid@example.com",
                Telephone = "9876543210"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/client", newClient);
            var createdClient = await DeserializeResponse<ClientResponseModel>(createResponse);

            // Act
            var response = await _client.GetAsync($"/api/client/{createdClient.ClientId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var client = await DeserializeResponse<ClientResponseModel>(response);
            client.ClientId.Should().Be(createdClient.ClientId);
            client.Name.Should().Be(newClient.Name);
            client.Email.Should().Be(newClient.Email);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var response = await _client.GetAsync($"/api/client/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingClient()
        {
            // Arrange
            var newClient = new ClientRequestModel
            {
                Name = "Test Put",
                Email = "put@example.com",
                Telephone = "1111111111"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/client", newClient);
            var createdClient = await DeserializeResponse<ClientResponseModel>(createResponse);

            var updatedClient = new ClientRequestModel
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                Telephone = "2222222222"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/client/{createdClient.ClientId}", updatedClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/client/{createdClient.ClientId}");
            var client = await DeserializeResponse<ClientResponseModel>(getResponse);
            client.Name.Should().Be(updatedClient.Name);
            client.Email.Should().Be(updatedClient.Email);
            client.Telephone.Should().Be(updatedClient.Telephone);
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var updatedClient = new ClientRequestModel
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                Telephone = "3333333333"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/client/{nonExistentId}", updatedClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var newClient = new ClientRequestModel
            {
                Name = "Test Put Invalid",
                Email = "putinvalid@example.com",
                Telephone = "4444444444"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/client", newClient);
            var createdClient = await DeserializeResponse<ClientResponseModel>(createResponse);

            var invalidClient = new ClientRequestModel
            {
                Name = "", // Nome inválido
                Email = "updated@example.com",
                Telephone = "5555555555"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/client/{createdClient.ClientId}", invalidClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesClient()
        {
            // Arrange
            var newClient = new ClientRequestModel
            {
                Name = "Test Delete",
                Email = "delete@example.com",
                Telephone = "6666666666"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/client", newClient);
            var createdClient = await DeserializeResponse<ClientResponseModel>(createResponse);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/client/{createdClient.ClientId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/client/{createdClient.ClientId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var response = await _client.DeleteAsync($"/api/client/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
