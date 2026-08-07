using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Application.Common.Interfaces;
using TutorMatchingPlatform.Application.Contracts.Auth;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return null!;
            }

            var passwordHash = _passwordHasher.HashPassword(request.Password);
            var user = new User(request.Email, passwordHash, request.FullName, null, null, (UserRole)request.Role);

            await _userRepository.AddAsync(user);

            // Create respective profile based on role
            if (user.Role == UserRole.Tutor)
            {
                var tutorProfile = new TutorProfile(user.Id, null, null);
                tutorProfile.Approve(user.Id); // Auto-approve so tutor appears in database search queries
                await _userRepository.AddTutorProfileAsync(tutorProfile);
            }
            else if (user.Role == UserRole.Student)
            {
                var studentProfile = new StudentProfile(user.Id, null, null);
                await _userRepository.AddStudentProfileAsync(studentProfile);
            }

            var accessToken = _jwtTokenGenerator.GenerateToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            user.SetRefreshToken(refreshToken, refreshTokenExpiryTime);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
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
