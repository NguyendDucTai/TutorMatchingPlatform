using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Auth.DTOs;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Interfaces.Authentication;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResult>
    {
        private readonly IAppDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandler(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthenticationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Check if email exists
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new Exception("MSG13"); // Email already registered
            }

            // 2. Hash password
            var hashedPassword = _passwordHasher.HashPassword(request.Password);

            // 3. Create user
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = request.Role,
                IsSuspended = false,
                CreditBalance = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Setup appropriate profile based on role
            if (request.Role == UserRole.Tutor)
            {
                user.TutorProfile = new TutorProfile
                {
                    Status = ProfileStatus.Pending,
                    HourlyRate = 0,
                    CreatedAt = DateTime.UtcNow
                };
            }
            else if (request.Role == UserRole.Student)
            {
                user.StudentProfile = new StudentProfile
                {
                    CreatedAt = DateTime.UtcNow
                };
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Generate JWT
            var token = _jwtTokenGenerator.GenerateToken(user);

            // 5. Return result
            return new AuthenticationResult
            {
                User = user,
                Token = token
            };
        }
    }
}
