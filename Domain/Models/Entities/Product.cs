using Domain.Interfaces;

namespace Domain.Models.Entities
{
    public class Product : BaseEntity, ISoftDeletable
    {
        public Product()
        {
            OrderItems = new List<OrderItem>();
        }

        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int ImageId { get; set; }
        public Image Image { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
