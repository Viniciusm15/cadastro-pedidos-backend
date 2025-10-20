namespace Domain.Models.RequestModels.AuthRequestModels
{
    public class RegisterRequestModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Telephone { get; set; }
        public required DateTime BirthDate { get; set; }
    }
}
