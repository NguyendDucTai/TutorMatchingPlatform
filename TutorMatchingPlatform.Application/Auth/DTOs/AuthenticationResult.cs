using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Auth.DTOs
{
    public class AuthenticationResult
    {
        public User User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
    }
}
