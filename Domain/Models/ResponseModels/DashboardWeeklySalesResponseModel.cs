namespace Domain.Models.ResponseModels
{
    public class DashboardWeeklySalesResponseModel
    {
        public required string DayOfWeek { get; set; }
        public double TotalSales { get; set; }
    }
}

