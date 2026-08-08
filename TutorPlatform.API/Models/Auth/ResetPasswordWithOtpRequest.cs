namespace TutorPlatform.API.Models.Auth
{
    public class ResetPasswordWithOtpRequest
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
