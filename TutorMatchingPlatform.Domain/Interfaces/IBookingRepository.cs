using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Booking>> GetByTutorIdAsync(Guid tutorId);
        Task<IReadOnlyList<Booking>> GetByStudentIdAsync(Guid studentId);
        Task<PagedResult<Booking>> GetPagedAsync(Guid userId, UserRole role, int pageNumber, int pageSize);
        Task<Booking> AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task<bool> HasOverlappingBookingAsync(Guid tutorId, DateTime start, DateTime end);
    }
}
