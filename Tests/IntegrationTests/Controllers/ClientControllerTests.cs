using Common.Models;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class ClientControllerTests : IntegrationTestBase
    {
        private readonly ClientTestHelper _clientHelper;

        public ClientControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _clientHelper = new ClientTestHelper(_client);
        }

        [Fact]
        public async Task GetAll_ReturnsPagedClients()
        {
            // Arrange
            await _clientHelper.CreateTestClient();

            // Act
            var getResponse = await _client.GetAsync("/api/client?pageNumber=1&pageSize=10");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<ClientResponseModel>>(getResponse);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewClient()
        {
            // Arrange
            var clientRequestModel = _clientHelper.CreateClientRequestModel();

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/client", clientRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse.Headers.Location.Should().NotBeNull();

            var clientResponseModel = await DeserializeResponse<ClientResponseModel>(postResponse);
            clientResponseModel.Name.Should().Be(clientRequestModel.Name);
            clientResponseModel.Email.Should().Be(clientRequestModel.Email);
            clientResponseModel.Telephone.Should().Be(clientRequestModel.Telephone);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidClientRequestModel = _clientHelper.CreateClientRequestModel(name: "");

            // Act
            var postResponse = await _client.PostAsJsonAsync("/api/client", invalidClientRequestModel);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedClient()
        {
            // Arrange
            var createdClient = await _clientHelper.CreateTestClient();

            // Act
            var getResponse = await _client.GetAsync($"/api/client/{createdClient.ClientId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var clientResponseModel = await DeserializeResponse<ClientResponseModel>(getResponse);
            clientResponseModel.ClientId.Should().Be(createdClient.ClientId);
            clientResponseModel.Name.Should().Be(createdClient.Name);
            clientResponseModel.Email.Should().Be(createdClient.Email);
            clientResponseModel.Telephone.Should().Be(createdClient.Telephone);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var getResponse = await _client.GetAsync($"/api/client/{nonExistentId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingClient()
        {
            // Arrange
            var createdClient = await _clientHelper.CreateTestClient();
            var updatedClientRequestModel = _clientHelper.CreateClientRequestModel(
                name: "Updated Name", email: "updated@example.com", telephone: "2222222222");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/client/{createdClient.ClientId}", updatedClientRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/client/{createdClient.ClientId}");
            var clientResponseModel = await DeserializeResponse<ClientResponseModel>(getResponse);
            clientResponseModel.Name.Should().Be(updatedClientRequestModel.Name);
            clientResponseModel.Email.Should().Be(updatedClientRequestModel.Email);
            clientResponseModel.Telephone.Should().Be(updatedClientRequestModel.Telephone);
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var clientRequestModel = _clientHelper.CreateClientRequestModel(
                name: "Updated", email: "updated@example.com", telephone: "000000000");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/client/{nonExistentId}", clientRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createdClient = await _clientHelper.CreateTestClient();
            var invalidClientRequestModel = _clientHelper.CreateClientRequestModel(name: "");

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/client/{createdClient.ClientId}", invalidClientRequestModel);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesClient()
        {
            // Arrange
            var createdClient = await _clientHelper.CreateTestClient();

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
            var deleteResponse = await _client.DeleteAsync($"/api/client/{nonExistentId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
