using Common.Models;
using Domain.Enums;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tests.IntegrationTests.Shared;

namespace Tests.IntegrationTests.Controllers
{
    public class OrdersControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_ReturnsPagedOrders()
        {
            // Act
            var response = await _client.GetAsync("/api/order?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await DeserializeResponse<PagedResult<OrderResponseModel>>(response);
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesNewOrder_WithProductAndImageRelationships()
        {
            // Arrange
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent("Nome do Produto"), "Name");
            formData.Add(new StringContent("Descrição detalhada"), "Description");
            formData.Add(new StringContent("99.99"), "Price");
            formData.Add(new StringContent("10"), "StockQuantity");
            formData.Add(new StringContent("1"), "CategoryId");
            formData.Add(new StringContent("Descrição da imagem"), "Image.Description");
            formData.Add(new StringContent("image/png"), "Image.ImageMimeType");

            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D,
                0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00,
                0x00
            };

            var imageStream = new MemoryStream(imageBytes);
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            formData.Add(imageContent, "Image.ImageData", "image.png");

            var productResponse = await _client.PostAsync("/api/product", formData);
            if (!productResponse.IsSuccessStatusCode)
            {
                var errorContent = await productResponse.Content.ReadAsStringAsync();
                var headers = productResponse.Headers.ToString();
                var requestContent = await formData.ReadAsStringAsync(); // Nota: pode não ser legível

                throw new Exception($"Erro ao criar produto:\nStatus: {productResponse.StatusCode}\nErro: {errorContent}\nHeaders: {headers}");
            }

            productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdProduct = await DeserializeResponse<Product>(productResponse);

            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = 199.98, 
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() {
                        ProductId = 1,
                        Quantity = 2,
                        UnitaryPrice = 99.99
                    }
                }
            };

            // Act
            var orderResponse = await _client.PostAsJsonAsync("/api/order", newOrder);

            // Assert
            orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(orderResponse);

            createdOrder.OrderItems.Should().ContainSingle();
            createdOrder.OrderItems.First().ProductId.Should().Be(1);
            createdOrder.TotalValue.Should().Be(199.98);
        }

        [Fact]
        public async Task Post_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidOrder = new OrderRequestModel
            {
                ClientId = 0,
                OrderItems = new List<OrderItemRequestModel>()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/order", invalidOrder);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsCreatedOrder()
        {
            // Arrange
            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = 199.98,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 3, UnitaryPrice = 10 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/order", newOrder);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(createResponse);

            // Act
            var response = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await DeserializeResponse<OrderResponseModel>(response);
            order.OrderId.Should().Be(createdOrder.OrderId);
            order.ClientId.Should().Be(newOrder.ClientId);
            order.OrderItems.Should().HaveCount(newOrder.OrderItems.Count());
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var response = await _client.GetAsync($"/api/order/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_UpdatesExistingOrder()
        {
            // Arrange
            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = 199.98,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 3, UnitaryPrice = 2 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/order", newOrder);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(createResponse);

            var updatedOrder = new OrderRequestModel
            {
                ClientId = 2,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = 199.98,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 2 },
                    new() { ProductId = 3, Quantity = 1 }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/order/{createdOrder.OrderId}", updatedOrder);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");
            var order = await DeserializeResponse<OrderResponseModel>(getResponse);
            order.ClientId.Should().Be(updatedOrder.ClientId);
            order.OrderItems.Should().HaveCount(updatedOrder.OrderItems.Count());
        }

        [Fact]
        public async Task Put_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var updatedOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/order/{nonExistentId}", updatedOrder);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderDate = DateTime.UtcNow.Date,
                TotalValue = 199.98,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 1, UnitaryPrice = 5 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/order", newOrder);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(createResponse);

            var invalidOrder = new OrderRequestModel
            {
                ClientId = 0,
                OrderItems = new List<OrderItemRequestModel>()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/order/{createdOrder.OrderId}", invalidOrder);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_RemovesOrder()
        {
            // Arrange
            var newOrder = new OrderRequestModel
            {
                ClientId = 1,
                OrderItems = new List<OrderItemRequestModel>
                {
                    new() { ProductId = 1, Quantity = 1 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/order", newOrder);
            var createdOrder = await DeserializeResponse<OrderResponseModel>(createResponse);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/order/{createdOrder.OrderId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/order/{createdOrder.OrderId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var response = await _client.DeleteAsync($"/api/order/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GenerateCsvReport_ReturnsCsvFile()
        {
            // Act
            var response = await _client.GetAsync("/api/order/generate-csv-report");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("Order_Report.csv");
        }
    }
}
