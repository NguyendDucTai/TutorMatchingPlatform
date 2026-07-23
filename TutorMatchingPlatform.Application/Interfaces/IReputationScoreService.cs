using System.Threading;
using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface IReputationScoreService
    {
        Task CalculateAndUpdateAsync(int userId, CancellationToken cancellationToken = default);
        Task RecalculateAllActiveUsersAsync(CancellationToken cancellationToken = default);
    }
}
