namespace Domain.Models.Entities
{
    public class Product : BaseEntity
    {
        public Product()
        {
            OrderItens = new List<OrderItem>();
        }

        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<OrderItem> OrderItens { get; set; }
    }
}
