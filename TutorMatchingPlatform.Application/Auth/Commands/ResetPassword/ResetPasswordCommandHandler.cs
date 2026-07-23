using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Interfaces.Authentication;

namespace TutorMatchingPlatform.Application.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

            // Validate token exists and is not expired
            if (user == null || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Message = "Invalid or expired password reset token."
                };
            }

            // Hash the new password and update user
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            // Invalidate the token after use (one-time use only)
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Also clear refresh tokens for security (force re-login on all devices)
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync(cancellationToken);

            return new ResetPasswordResult
            {
                Success = true,
                Message = "Password has been reset successfully. Please log in with your new password."
            };
        }
    }
}
