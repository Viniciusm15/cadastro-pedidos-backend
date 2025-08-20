using Common.Models;
using Domain.Models.ResponseModels;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseModel> GetDashboardMetricsAsync();
        Task<List<DashboardWeeklySalesResponseModel>> GetWeeklySalesDataAsync(DateTime startDate, DateTime endDate);
        Task<PagedResult<DashboardLowStockProductResponseModel>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<DashboardPendingOrderResponseModel>> GetPendingOrdersAsync(int pageNumber = 1, int pageSize = 10);
        Task<DashboardClientSummaryResponseModel> GetClientDataAsync();
        Task<DashboardRestockResponseModel> RestockProductAsync(int productId, int restockQuantity);
    }
}
