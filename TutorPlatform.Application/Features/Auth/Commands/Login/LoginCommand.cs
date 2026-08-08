using MediatR;
using TutorPlatform.Application.Contracts.Auth;

namespace TutorPlatform.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<AuthResponse>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
