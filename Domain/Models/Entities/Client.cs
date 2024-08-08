using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Entities
{
    public class Client : BaseEntity
    {
        public Client()
        {
            Orders = new List<Order>();
        }

        public required string Name { get; set; }
        public required string Email { get; set; }

        [RegularExpression(@"^\+\d{1,3}\s?\(\d{2}\)\s?\d{4,5}\-\d{4}$", ErrorMessage = "Número de telefone inválido")]
        public required string Telephone { get; set; }
        // +55 (47) 99141-0923
        // (47) 991410923
        // 47991410923
        public DateTime BirthDate { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
