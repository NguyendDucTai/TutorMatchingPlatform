using MediatR;
using TutorPlatform.Application.Contracts.Auth;

namespace TutorPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<AuthResponse>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int Role { get; set; } // 1 for Tutor, 2 for Student
    }
}
