using System.Net.Http.Headers;
using System.Text.Json;

namespace Tests.IntegrationTests.Configuration
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient _client;
        protected readonly CustomWebApplicationFactory _factory;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected internal static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}. Response: {content}");
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result is null ? throw new JsonException($"Deserialization returned null for content: {content}") : result;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize: {content}", ex);
            }
        }
    }
}
