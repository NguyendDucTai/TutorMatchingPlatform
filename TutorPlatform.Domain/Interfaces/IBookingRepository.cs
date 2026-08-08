using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Interfaces
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
        Task<bool> HasConfirmedOverlappingBookingAsync(Guid tutorId, DateTime start, DateTime end);
        Task<bool> HasStudentConfirmedOverlappingBookingAsync(Guid studentId, DateTime start, DateTime end);
        Task<bool> HasStudentActiveBookingForSubjectAsync(Guid studentId, DateTime start, DateTime end, Guid subjectId);
    }
}
