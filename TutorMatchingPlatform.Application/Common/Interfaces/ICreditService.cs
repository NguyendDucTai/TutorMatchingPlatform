using System;
using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Common.Interfaces
{
    public interface ICreditService
    {
        Task DebitAsync(Guid userId, decimal amount, string description, Guid? bookingId);
        Task TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description, Guid bookingId);
        Task RefundAsync(Guid userId, decimal amount, string description, Guid bookingId);
    }
}
