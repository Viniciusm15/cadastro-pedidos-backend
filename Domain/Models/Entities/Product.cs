namespace Domain.Models.Entities
{
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
