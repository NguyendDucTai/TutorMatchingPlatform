using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Auth.DTOs;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Interfaces.Authentication;

namespace TutorMatchingPlatform.Application.Auth.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthenticationResult>
    {
        private readonly IAppDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public LoginQueryHandler(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthenticationResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new Exception("MSG09"); // Incorrect email or password
            }

            if (user.IsSuspended)
            {
                throw new Exception("MSG10"); // Suspended/Locked
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                throw new Exception("MSG10"); // Account temporarily locked
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                    await _context.SaveChangesAsync(cancellationToken);
                    throw new Exception("MSG10"); // Account temporarily locked due to failed attempts
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                throw new Exception("MSG09"); // Incorrect email or password
            }

            // Successful login, reset lockout
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            var token = _jwtTokenGenerator.GenerateToken(user);
            
            // Generate Refresh Token
            user.RefreshToken = Guid.NewGuid().ToString("N");
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync(cancellationToken);

            return new AuthenticationResult
            {
                User = user,
                Token = token,
                RefreshToken = user.RefreshToken
            };
        }
    }
}
