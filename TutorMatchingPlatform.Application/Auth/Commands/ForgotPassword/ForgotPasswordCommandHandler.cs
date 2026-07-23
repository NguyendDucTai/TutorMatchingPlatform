using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IAppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                return new ForgotPasswordResult
                {
                    Success = true,
                    Message = "If this email is registered, a password reset link has been sent."
                };
            }

            // Generate a cryptographically secure token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                               .Replace("+", "-").Replace("/", "_").Replace("=", "");

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry

            await _context.SaveChangesAsync(cancellationToken);

            // Send email asynchronously
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            return new ForgotPasswordResult
            {
                Success = true,
                Message = "If this email is registered, a password reset link has been sent.",
                // Return token for development/testing convenience (remove in production)
                ResetToken = token
            };
        }
    }
}
