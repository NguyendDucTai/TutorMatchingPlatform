using System;
using System.Threading.Tasks;

namespace TutorPlatform.Domain.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task CreateTokenAsync(Guid userId, string email, string tokenHash, TimeSpan validDuration);
        Task<bool> ValidateAndConsumeTokenAsync(string email, string tokenHash);
    }
}
