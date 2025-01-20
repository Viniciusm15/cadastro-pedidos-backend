using Domain.Enums;

namespace Domain.Models.Base
{
    public abstract class OrderBaseModel
    {
        public DateTime OrderDate { get; set; }
        public double TotalValue { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int ClientId { get; set; }
    }
}
