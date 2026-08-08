using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Application.Contracts.Auth;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(request.AccessToken))
            {
                throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("AccessToken", "Invalid access token format.") });
            }

            var jwtToken = handler.ReadJwtToken(request.AccessToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub");
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("AccessToken", "Invalid access token payload.") });
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new ForbiddenException("Invalid or expired refresh token.");
            }

            var newAccessToken = _jwtTokenGenerator.GenerateToken(user);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            
            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = (int)user.Role,
                    AvatarUrl = user.AvatarUrl
                }
            };
        }
    }
}
