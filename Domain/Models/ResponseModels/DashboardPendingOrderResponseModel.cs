using Domain.Enums;

namespace Domain.Models.ResponseModels
{
    public class DashboardPendingOrderResponseModel
    {
        public required string Id { get; set; }
        public required string ClientName { get; set; }
        public double Amount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime Date { get; set; }
    }
}
