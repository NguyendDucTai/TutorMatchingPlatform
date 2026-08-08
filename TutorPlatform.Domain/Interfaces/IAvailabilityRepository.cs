using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface IAvailabilityRepository
    {
        Task<IReadOnlyList<Availability>> GetByUserIdAsync(Guid userId);
        Task BulkReplaceAsync(Guid userId, List<Availability> newAvailabilities);
        Task<bool> HasConflictingBookingsAsync(Guid userId, List<Availability> newAvailabilities);
    }
}
