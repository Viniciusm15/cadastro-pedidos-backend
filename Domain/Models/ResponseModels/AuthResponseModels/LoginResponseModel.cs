namespace Domain.Models.ResponseModels.AuthResponseModels
{
    public class LoginResponseModel
    {
        public required string Token { get; set; }
        public DateTime ExpiresIn { get; set; }
        public UserResponseModel User { get; set; } = null!;
    }
}
