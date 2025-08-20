namespace Domain.Models.ResponseModels
{
    public class DashboardClientSummaryResponseModel
    {
        public int TotalClients { get; set; }
        public int NewClientsThisMonth { get; set; }
        public int RetentionRate { get; set; }
        public List<int> MonthlyData { get; set; } = new List<int>();
    }
}
