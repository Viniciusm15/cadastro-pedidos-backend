using System.ComponentModel.DataAnnotations;

namespace Domain.Models.RequestModels
{
    public class ClientRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Telephone is required")]
        public required string Telephone { get; set; }

        public DateTime BirthDate { get; set; }
    }
}
