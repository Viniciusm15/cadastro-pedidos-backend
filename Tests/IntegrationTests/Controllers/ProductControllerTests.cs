using Common.Models;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class ProductControllerTests : IntegrationTestBase
    {
        private readonly ProductTestHelper _productHelper;

        public ProductControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _productHelper = new ProductTestHelper(factory);
        }

        [Fact]
        public async Task GetProducts_ReturnsPagedProducts()
        {
            var response = await _client.GetAsync("/api/product?pageNumber=1&pageSize=10");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<ProductResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            var createdProduct = await _productHelper.CreateTestProduct();

            var response = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var product = await DeserializeResponse<ProductResponseModel>(response);
            product.ProductId.Should().Be(createdProduct.ProductId);
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct()
        {
            var productRequest = await _productHelper.CreateProductRequestModel();

            var content = _productHelper.CreateMultipartFormDataContent(productRequest);
            var response = await _client.PostAsync("/api/product", content);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdProduct = await DeserializeResponse<ProductResponseModel>(response);
            createdProduct.Name.Should().Be(productRequest.Name);
            createdProduct.Description.Should().Be(productRequest.Description);
            createdProduct.Price.Should().Be(productRequest.Price);
            createdProduct.StockQuantity.Should().Be(productRequest.StockQuantity);
            createdProduct.CategoryId.Should().Be(productRequest.CategoryId);
        }

        [Fact]
        public async Task PutProduct_UpdatesExistingProduct()
        {
            var createdProduct = await _productHelper.CreateTestProduct();

            var updatedProductRequest = await _productHelper.CreateProductRequestModel(
                name: "Updated Product",
                description: "Updated Description",
                price: 20.99,
                stockQuantity: 50,
                categoryId: createdProduct.CategoryId,
                imageDescription: "Updated Image Description"
            );

            var updatedContent = _productHelper.CreateMultipartFormDataContent(updatedProductRequest);

            var response = await _client.PutAsync($"/api/product/{createdProduct.ProductId}", updatedContent);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");
            var updatedProduct = await DeserializeResponse<ProductResponseModel>(getResponse);

            updatedProduct.Name.Should().Be(updatedProductRequest.Name);
            updatedProduct.Description.Should().Be(updatedProductRequest.Description);
        }

        [Fact]
        public async Task DeleteProductById_RemovesProduct()
        {
            var createdProduct = await _productHelper.CreateTestProduct();

            var deleteResponse = await _client.DeleteAsync($"/api/product/{createdProduct.ProductId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
