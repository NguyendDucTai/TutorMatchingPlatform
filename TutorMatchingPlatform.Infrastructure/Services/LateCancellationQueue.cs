using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.Services
{
    public class LateCancellationQueue : ILateCancellationQueue
    {
        private readonly Channel<int> _queue;

        public LateCancellationQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<int>(options);
        }

        public async ValueTask QueueUserForCheckAsync(int userId, CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(userId, cancellationToken);
        }

        public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
