namespace Domain.Models.ResponseModels
{
    public class DashboardResponseModel
    {
        public double TotalSales { get; set; }
        public int SalesChangePercentage { get; set; }
        public int LowStockProductsCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int ActiveClientsCount { get; set; }
        public int NewClientsThisMonth { get; set; }
    }
}
