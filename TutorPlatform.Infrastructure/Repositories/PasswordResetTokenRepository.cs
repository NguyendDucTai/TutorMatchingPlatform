using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PasswordResetTokenRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateTokenAsync(Guid userId, string email, string tokenHash, TimeSpan validDuration)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var oldTokens = await _dbContext.PasswordResetTokens
                .Where(t => t.Email == normalizedEmail)
                .ToListAsync();

            if (oldTokens.Any())
            {
                _dbContext.PasswordResetTokens.RemoveRange(oldTokens);
            }

            var newToken = new PasswordResetTokenDataModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = normalizedEmail,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.Add(validDuration),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.PasswordResetTokens.Add(newToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ValidateAndConsumeTokenAsync(string email, string tokenHash)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var tokenRecord = await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == normalizedEmail && t.TokenHash == tokenHash);

            if (tokenRecord == null || tokenRecord.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            _dbContext.PasswordResetTokens.Remove(tokenRecord);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
