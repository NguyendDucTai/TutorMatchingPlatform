using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpCommandHandler : IRequestHandler<ResetPasswordWithOtpCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordWithOtpCommandHandler(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ResetPasswordWithOtpCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var tokenHash = HashCode(request.OtpCode.Trim());

            // 1. Validate and consume token from DB
            var isValid = await _passwordResetTokenRepository.ValidateAndConsumeTokenAsync(normalizedEmail, tokenHash);
            if (!isValid)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("OtpCode", "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử gửi lại mã.")
                });
            }

            // 2. Fetch User
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin tài khoản.");
            }

            // 3. Hash new password using BCrypt password hasher
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);

            // 4. Update user password in DB
            await _userRepository.UpdateAsync(user);

            return true;
        }

        private static string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code.Trim()));
            return Convert.ToHexString(bytes);
        }
    }
}
