using MediatR;
using TutorMatchingPlatform.Application.Auth.DTOs;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<AuthenticationResult>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
