namespace Domain.Models.Entities
{
    public class Category : BaseEntity
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public required string Name { get; set; }
        public required string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
