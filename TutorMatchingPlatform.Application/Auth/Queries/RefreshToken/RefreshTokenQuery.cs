using MediatR;
using TutorMatchingPlatform.Application.Auth.DTOs;

namespace TutorMatchingPlatform.Application.Auth.Queries.RefreshToken
{
    public class RefreshTokenQuery : IRequest<AuthenticationResult>
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
