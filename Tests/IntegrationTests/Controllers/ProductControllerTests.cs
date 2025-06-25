using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tests.IntegrationTests.Configuration;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class ProductControllerTests : IntegrationTestBase
    {
        private readonly ProductTestHelper _productHelper;

        public ProductControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _productHelper = new ProductTestHelper(_client);
        }

        [Fact]
        public async Task GetAll_ReturnsPagedProducts()
        {
            // Arrange
            await _productHelper.CreateTestProduct();

            // Act
            var getResponse = await _client.GetAsync("/api/product?pageNumber=1&pageSize=10");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<ProductResponseModel>>(getResponse);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewProduct()
        {
            // Arrange
            var productRequestModel = _productHelper.CreateProductRequestModel(categoryId: 1);
            var postContent = _productHelper.CreateMultipartFormDataContent(productRequestModel);

            // Act
            var postResponse = await _client.PostAsync("/api/product", postContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse.Headers.Location.Should().NotBeNull();

            var productResponseModel = await DeserializeResponse<ProductResponseModel>(postResponse);
            productResponseModel.Name.Should().Be(productRequestModel.Name);
            productResponseModel.Description.Should().Be(productRequestModel.Description);
            productResponseModel.Price.Should().Be(productRequestModel.Price);
            productResponseModel.StockQuantity.Should().Be(productRequestModel.StockQuantity);
            productResponseModel.CategoryId.Should().Be(productRequestModel.CategoryId);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var productRequestModel = _productHelper.CreateProductRequestModel(categoryId: 1, name: "");
            var postContent = _productHelper.CreateMultipartFormDataContent(productRequestModel);

            // Act
            var postResponse = await _client.PostAsync("/api/product", postContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsProduct()
        {
            // Arrange
            var createdProduct = await _productHelper.CreateTestProduct();

            // Act
            var getResponse = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var productResponseModel = await DeserializeResponse<ProductResponseModel>(getResponse);
            productResponseModel.ProductId.Should().Be(createdProduct.ProductId);
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var getResponse = await _client.GetAsync($"/api/product/{nonExistentId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingProduct()
        {
            // Arrange
            var createdProduct = await _productHelper.CreateTestProduct();

            var updatedProductRequestModel = _productHelper.CreateProductRequestModel(
                name: "Updated Product",
                description: "Updated Description",
                price: 20.99,
                stockQuantity: 50,
                categoryId: createdProduct.CategoryId,
                imageDescription: "Updated Image Description");

            var putContent = _productHelper.CreateMultipartFormDataContent(updatedProductRequestModel);

            // Act
            var putResponse = await _client.PutAsync($"/api/product/{createdProduct.ProductId}", putContent);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");
            var productResponseModel = await DeserializeResponse<ProductResponseModel>(getResponse);
            productResponseModel.Name.Should().Be(updatedProductRequestModel.Name);
            productResponseModel.Description.Should().Be(updatedProductRequestModel.Description);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createdProduct = await _productHelper.CreateTestProduct();
            var invalidProductRequestModel = _productHelper.CreateProductRequestModel(name: "");
            var putContent = _productHelper.CreateMultipartFormDataContent(invalidProductRequestModel);

            // Act
            var putResponse = await _client.PutAsync($"/api/product/{createdProduct.CategoryId}", putContent);

            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesProduct()
        {
            // Arrange
            var createdProduct = await _productHelper.CreateTestProduct();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/product/{createdProduct.ProductId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/product/{createdProduct.ProductId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/product/{nonExistentId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
