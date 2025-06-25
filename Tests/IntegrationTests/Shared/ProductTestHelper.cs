using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using System.Globalization;
using Tests.IntegrationTests.Configuration;

namespace Tests.IntegrationTests.Shared
{
    public class ProductTestHelper(HttpClient client)
    {
        private readonly HttpClient _client = client;
        private readonly CategoryTestHelper _categoryHelper = new(client);

        public ProductRequestModel CreateProductRequestModel(
            string name = "Test Product",
            string description = "Test Description",
            double price = 10.99,
            int stockQuantity = 100,
            int categoryId = 0,
            string imageDescription = "Test Image Description")
        {
            return new ProductRequestModel
            {
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                CategoryId = categoryId,
                Image = CreateTestImage(imageDescription)
            };
        }

        public async Task<ProductRequestModel> CreateProductRequestModelWithCategoryAsync(
            string name = "Test Product",
            string description = "Test Description",
            double price = 10.99,
            int stockQuantity = 100,
            string imageDescription = "Test Image Description")
        {
            var category = await _categoryHelper.CreateTestCategory();

            return new ProductRequestModel
            {
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                CategoryId = category.CategoryId,
                Image = CreateTestImage(imageDescription)
            };
        }

        public ImageRequestModel CreateTestImage(string description = "Test Image Description")
        {
            var imageData = new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
                0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xFF, 0xFF, 0x3F,
                0x00, 0x05, 0xFE, 0x02, 0xFE, 0xDC, 0xCC, 0x59, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44,
                0xAE, 0x42, 0x60, 0x82
            };

            return new ImageRequestModel
            {
                Description = description,
                ImageMimeType = "image/png",
                ImageData = imageData
            };
        }

        public MultipartFormDataContent CreateMultipartFormDataContent(ProductRequestModel product)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(product.Name), "name" },
                { new StringContent(product.Description), "description" },
                { new StringContent(product.Price.ToString(CultureInfo.InvariantCulture)), "price" },
                { new StringContent(product.StockQuantity.ToString()), "stockQuantity" },
                { new StringContent(product.CategoryId.ToString()), "categoryId" },
                { new StringContent(product.Image.Description), "Image.Description" },
                { new StringContent(product.Image.ImageMimeType), "Image.ImageMimeType" }
            };

            var base64String = Convert.ToBase64String(product.Image.ImageData);
            content.Add(new StringContent(base64String), "Image.ImageData");

            return content;
        }

        public async Task<ProductResponseModel> CreateTestProduct(
            string name = "Test Product",
            string description = "Test Description",
            double price = 10.99,
            int stockQuantity = 100,
            string imageDescription = "Test Image Description")
        {
            var productRequest = await CreateProductRequestModelWithCategoryAsync(name, description, price, stockQuantity, imageDescription);
            var content = CreateMultipartFormDataContent(productRequest);

            var response = await _client.PostAsync("/api/product", content);
            return await IntegrationTestBase.DeserializeResponse<ProductResponseModel>(response);
        }
    }
}
