namespace Domain.Models.Reports
{
    public class OrderReportModel
    {
        public int OrderNumber { get; set; }
        public string ClientName { get; set; }
        public string OrderDate { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }
        public double TotalValue { get; set; }
    }
}
