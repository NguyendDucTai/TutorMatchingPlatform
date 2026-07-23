using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.Services
{
    public class ReputationScoreService : IReputationScoreService
    {
        private readonly IAppDbContext _context;

        public ReputationScoreService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task CalculateAndUpdateAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return;

            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var feedbacks = await _context.Feedbacks
                .Where(f => f.ReceiverId == userId && f.CreatedAt >= ninetyDaysAgo)
                .ToListAsync(cancellationToken);

            if (!feedbacks.Any())
            {
                user.ReputationScore = null; // empty until first rating
            }
            else
            {
                double totalScore = 0;
                double totalWeight = 0;

                foreach (var feedback in feedbacks)
                {
                    double weight = feedback.CreatedAt >= thirtyDaysAgo ? 1.0 : 0.5;
                    totalScore += feedback.Rating * weight;
                    totalWeight += weight;
                }

                user.ReputationScore = totalWeight > 0 ? Math.Round(totalScore / totalWeight, 2) : null;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RecalculateAllActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            
            // Get all users who received feedback in the last 90 days
            var activeUserIds = await _context.Feedbacks
                .Where(f => f.CreatedAt >= ninetyDaysAgo)
                .Select(f => f.ReceiverId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var userId in activeUserIds)
            {
                await CalculateAndUpdateAsync(userId, cancellationToken);
            }
        }
    }
}
