using System.Threading;
using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface ILateCancellationQueue
    {
        ValueTask QueueUserForCheckAsync(int userId, CancellationToken cancellationToken = default);
        ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
    }
}
