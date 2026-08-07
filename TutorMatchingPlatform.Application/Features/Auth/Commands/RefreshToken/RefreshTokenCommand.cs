using MediatR;
using TutorMatchingPlatform.Application.Contracts.Auth;

namespace TutorMatchingPlatform.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthResponse>
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
