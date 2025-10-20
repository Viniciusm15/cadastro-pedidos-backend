namespace Domain.Models.ResponseModels.AuthResponseModels
{
    public class UserResponseModel
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public List<string> Roles { get; set; } = [];
        public required int ClientId { get; set; }
    }
}
