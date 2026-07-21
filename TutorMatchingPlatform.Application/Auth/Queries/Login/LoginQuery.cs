using MediatR;
using TutorMatchingPlatform.Application.Auth.DTOs;

namespace TutorMatchingPlatform.Application.Auth.Queries.Login
{
    public class LoginQuery : IRequest<AuthenticationResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
