namespace Domain.Models.ResponseModels
{
    public class DashboardLowStockProductResponseModel
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public int StockQuantity { get; set; }
        public required string CategoryName { get; set; }
    }
}
