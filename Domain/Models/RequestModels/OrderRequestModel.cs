using System.ComponentModel.DataAnnotations;

namespace Domain.Models.RequestModels
{
    public class OrderRequestModel
    {
        [Required(ErrorMessage = "OrderDate is required")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "TotalValue is required")]
        public double TotalValue { get; set; }

        [Required(ErrorMessage = "ClientId is required")]
        public int ClientId { get; set; }
    }
}
