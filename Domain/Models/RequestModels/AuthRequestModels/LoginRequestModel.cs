namespace Domain.Models.RequestModels.AuthRequestModels
{
    public class LoginRequestModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
