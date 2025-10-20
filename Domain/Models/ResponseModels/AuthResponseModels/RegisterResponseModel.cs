namespace Domain.Models.ResponseModels.AuthResponseModels
{
    public class RegisterResponseModel
    {
        public required string Token { get; set; }
        public UserResponseModel User { get; set; } = null!;
    }
}
