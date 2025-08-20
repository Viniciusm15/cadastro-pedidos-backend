using Application.Interfaces;
using Common.Models;
using Domain.Models.ResponseModels;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IClientService _clientService;

        public DashboardService(ILogger<DashboardService> logger, IOrderService orderService, IProductService productService, IClientService clientService)
        {
            _logger = logger;
            _orderService = orderService;
            _productService = productService;
            _clientService = clientService;
        }

        public async Task<DashboardResponseModel> GetDashboardMetricsAsync()
        {
            var totalOrdersSales = await _orderService.GetTotalOrderSalesAsync();
            var orderSalesTrend = await _orderService.GetOrderSalesTrendAsync();
            var lowStockProducts = await _productService.GetLowStockProductsCountAsync();
            var pendingOrdersCount = await _orderService.GetPendingOrdersCountAsync();
            var activeClientsCount = await _clientService.GetActiveClientsCountAsync();
            var newClientsThisMonth = await _clientService.GetNewClientsThisMonthAsync();

            return new DashboardResponseModel
            {
                TotalSales = totalOrdersSales,
                SalesChangePercentage = orderSalesTrend.ChangePercentage,
                LowStockProductsCount = lowStockProducts,
                PendingOrdersCount = pendingOrdersCount,
                ActiveClientsCount = activeClientsCount,
                NewClientsThisMonth = newClientsThisMonth
            };
        }

        public async Task<List<DashboardWeeklySalesResponseModel>> GetWeeklySalesDataAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Getting weekly sales data from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                startDate, endDate);

            var daysMap = new Dictionary<DayOfWeek, string>
            {
                [DayOfWeek.Sunday] = "Dom",
                [DayOfWeek.Monday] = "Seg",
                [DayOfWeek.Tuesday] = "Ter",
                [DayOfWeek.Wednesday] = "Qua",
                [DayOfWeek.Thursday] = "Qui",
                [DayOfWeek.Friday] = "Sex",
                [DayOfWeek.Saturday] = "Sáb"
            };

            var result = (await _orderService.GetOrdersByDateRangeAsync(startDate, endDate))
                .GroupBy(o => o.OrderDate.DayOfWeek)
                .Select(x => new
                {
                    DayName = daysMap[x.Key],
                    TotalSales = x.Sum(o => o.TotalValue)
                })
                .ToList();

            return Enumerable.Range(0, 7)
                .Select(i => new DashboardWeeklySalesResponseModel
                {
                    DayOfWeek = daysMap[(DayOfWeek)i],
                    TotalSales = result.FirstOrDefault(d => d.DayName == daysMap[(DayOfWeek)i])?.TotalSales ?? 0
                })
                .OrderBy(x => x.DayOfWeek)
                .ToList();
        }

        public async Task<PagedResult<DashboardLowStockProductResponseModel>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Getting low stock products for page {PageNumber} with size {PageSize}",
                pageNumber, pageSize);

            return await _productService.GetLowStockProductsAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<DashboardPendingOrderResponseModel>> GetPendingOrdersAsync(int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Getting pending orders for page {PageNumber} with size {PageSize}",
                pageNumber, pageSize);

            return await _orderService.GetPendingOrdersAsync(pageNumber, pageSize);
        }

        public async Task<DashboardClientSummaryResponseModel> GetClientDataAsync()
        {
            _logger.LogInformation("Getting client dashboard data");
            return await _clientService.GetClientDataAsync();
        }

        public async Task<DashboardRestockResponseModel> RestockProductAsync(int productId, int restockQuantity)
        {
            _logger.LogInformation("Processing restock for product {ProductId}", productId);
            return await _productService.RestockProductAsync(productId, restockQuantity);
        }
    }
}
