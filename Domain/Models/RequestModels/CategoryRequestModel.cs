using System.ComponentModel.DataAnnotations;

namespace Domain.Models.RequestModels
{
    public class CategoryRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }
    }
}
