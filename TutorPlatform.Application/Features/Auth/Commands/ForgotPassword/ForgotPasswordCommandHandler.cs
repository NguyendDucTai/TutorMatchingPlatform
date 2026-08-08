using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null)
            {
                throw new NotFoundException("Email này chưa được đăng ký trên hệ thống.");
            }

            // Security check: Only Tutor (Role 1) and Student (Role 2) are supported
            if (user.Role == UserRole.Admin)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Email", "Tài khoản Admin không hỗ trợ khôi phục mật khẩu qua Email.")
                });
            }

            // 1. Generate 6-digit OTP code
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 2. Hash OTP code using SHA-256 for DB storage
            var tokenHash = HashCode(otpCode);

            // 3. Save token in DB using repository (valid for 10 minutes)
            await _passwordResetTokenRepository.CreateTokenAsync(user.Id, normalizedEmail, tokenHash, TimeSpan.FromMinutes(10));

            // 4. Build HTML Email Body with prominent 6-digit OTP code
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 520px; margin: 0 auto; padding: 24px; background-color: #0f172a; color: #f8fafc; border-radius: 16px; border: 1px solid rgba(255,255,255,0.1);'>
                    <h2 style='color: #38bdf8; text-align: center; margin-top: 0;'>🔐 Khôi Phục Mật Khẩu</h2>
                    <p style='font-size: 15px; color: #cbd5e1;'>Xin chào <strong>{user.FullName}</strong>,</p>
                    <p style='font-size: 14px; color: #cbd5e1;'>Bạn vừa yêu cầu mã xác nhận để đặt lại mật khẩu cho tài khoản <strong>{normalizedEmail}</strong> trên hệ thống TutorMatching.</p>
                    
                    <div style='background-color: rgba(56, 189, 248, 0.1); border: 2px dashed #38bdf8; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0;'>
                        <span style='font-size: 13px; color: #94a3b8; display: block; margin-bottom: 8px;'>MÃ XÁC THỰC OTP CỦA BẠN (HẠN DÙNG 10 PHÚT)</span>
                        <span style='font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #38bdf8; font-family: monospace;'>{otpCode}</span>
                    </div>

                    <p style='font-size: 13px; color: #94a3b8; font-style: italic;'>⚠️ Vui lòng không chia sẻ mã này cho bất kỳ ai. Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email.</p>
                    <hr style='border: none; border-top: 1px solid rgba(255,255,255,0.1); margin: 20px 0;' />
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>TutorMatching Platform &copy; 2026</p>
                </div>";

            await _emailService.SendEmailAsync(normalizedEmail, "Mã xác nhận khôi phục mật khẩu - TutorMatching", htmlBody);

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
