using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;

namespace Tests.IntegrationTests.Shared
{
    public class ClientTestHelper(HttpClient client)
    {
        private readonly HttpClient _client = client;

        public ClientRequestModel CreateClientRequestModel(
            string name = "Test Client",
            string? email = "testemail@example.com",
            string telephone = "47991234567")
        {
            return new ClientRequestModel
            {
                Name = name,
                Email = email ?? $"client_{Guid.NewGuid()}@email.com",
                Telephone = telephone
            };
        }

        public async Task<ClientResponseModel> CreateTestClient(
            string name = "Test Client",
            string? email = "testemail@example.com",
            string telephone = "47991234567")
        {
            var request = CreateClientRequestModel(name, email, telephone);

            var response = await _client.PostAsJsonAsync("/api/client", request);
            return await IntegrationTestBase.DeserializeResponse<ClientResponseModel>(response);
        }
    }
}
