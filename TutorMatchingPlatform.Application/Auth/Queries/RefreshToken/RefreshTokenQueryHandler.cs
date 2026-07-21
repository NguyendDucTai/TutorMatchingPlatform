using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Auth.DTOs;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Interfaces.Authentication;

namespace TutorMatchingPlatform.Application.Auth.Queries.RefreshToken
{
    public class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, AuthenticationResult>
    {
        private readonly IAppDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenQueryHandler(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthenticationResult> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
        {
            // For a robust implementation, we should also validate the access token (even if expired).
            // Here, we just find the user by the RefreshToken and check its expiry.
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired refresh token.");
            }

            var newToken = _jwtTokenGenerator.GenerateToken(user);
            user.RefreshToken = Guid.NewGuid().ToString("N");
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync(cancellationToken);

            return new AuthenticationResult
            {
                User = user,
                Token = newToken,
                RefreshToken = user.RefreshToken
            };
        }
    }
}
