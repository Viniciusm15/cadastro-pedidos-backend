namespace Domain.Models.RequestModels
{
    public class ProductRequestModel
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
    }
}
