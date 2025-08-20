namespace Domain.Models.ResponseModels
{
    public class DashboardRestockResponseModel
    {
        public required string Message { get; set; }
        public int NewStockQuantity { get; set; }
    }
}
