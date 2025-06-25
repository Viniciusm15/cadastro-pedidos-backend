using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;

namespace Tests.IntegrationTests.Shared
{
    public class CategoryTestHelper(HttpClient client)
    {
        private readonly HttpClient _client = client;

        public CategoryRequestModel CreateCategoryRequestModel(string name = "Test Category", string description = "Test Description")
        {
            return new CategoryRequestModel
            {
                Name = name,
                Description = description
            };
        }

        public async Task<CategoryResponseModel> CreateTestCategory(string name = "Test Category", string description = "Test Description")
        {
            var request = CreateCategoryRequestModel(name, description);

            var response = await _client.PostAsJsonAsync("/api/category", request);
            return await IntegrationTestBase.DeserializeResponse<CategoryResponseModel>(response);
        }
    }
}
