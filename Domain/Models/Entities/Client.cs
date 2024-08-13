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
        public required string Telephone { get; set; }
        public DateTime BirthDate { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
