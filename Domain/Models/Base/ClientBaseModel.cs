namespace Domain.Models.Base
{
    public abstract class ClientBaseModel
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Telephone { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
