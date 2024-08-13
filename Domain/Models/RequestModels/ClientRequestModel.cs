namespace Domain.Models.RequestModels
{
    public class ClientRequestModel
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Telephone { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
