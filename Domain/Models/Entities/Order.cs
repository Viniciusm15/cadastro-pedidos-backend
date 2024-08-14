using Domain.Interfaces;

namespace Domain.Models.Entities
{
    public class Order : BaseEntity, ISoftDeletable
    {
        public Order()
        {
            OrderItens = new List<OrderItem>();
        }

        public DateTime OrderDate { get; set; }
        public double TotalValue { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public ICollection<OrderItem> OrderItens { get; set; }
    }
}
