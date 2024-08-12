using System.ComponentModel.DataAnnotations;

namespace Domain.Models.RequestModels
{
    public class ProductRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public double Price { get; set; }

        [Required(ErrorMessage = "StockQuantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be zero or greater")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }
    }
}
