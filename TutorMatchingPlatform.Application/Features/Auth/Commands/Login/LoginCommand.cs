using MediatR;
using TutorMatchingPlatform.Application.Contracts.Auth;

namespace TutorMatchingPlatform.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<AuthResponse>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
