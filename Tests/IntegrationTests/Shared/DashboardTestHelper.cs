using Domain.Enums;

namespace Tests.IntegrationTests.Shared
{
    public class DashboardTestHelper(HttpClient client)
    {
        private readonly HttpClient _client = client;
        private readonly OrderTestHelper _orderHelper = new(client);
        private readonly ProductTestHelper _productHelper = new(client);
        private readonly ClientTestHelper _clientHelper = new(client);

        public async Task PrepareDashboardTestData()
        {
            await CreateTestOrderForWeeklySales();
        }

        public async Task CreateTestOrderForWeeklySales()
        {
            await _orderHelper.CreateTestOrder(quantity: 10);
        }

        public async Task CreateLowStockProduct()
        {
            await _productHelper.CreateTestProduct(stockQuantity: 3);
        }

        public async Task CreatePendingOrder()
        {
            await _orderHelper.CreateTestOrder(status: OrderStatus.Pending);
        }

        public async Task CreateClientForDashboard()
        {
            await _clientHelper.CreateTestClient();
        }

        public async Task<int> CreateTestProduct(int stockQuantity = 10)
        {
            var productResponseModel = await _productHelper.CreateTestProduct(stockQuantity: stockQuantity);
            return productResponseModel.ProductId;
        }
    }
}
