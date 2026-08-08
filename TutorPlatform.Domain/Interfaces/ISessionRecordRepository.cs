using System;
using System.Threading.Tasks;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface ISessionRecordRepository
    {
        Task<SessionRecord?> GetByBookingIdAsync(Guid bookingId);
        Task<IReadOnlyList<SessionRecord>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId);
        Task<SessionRecord> AddAsync(SessionRecord record);
        Task UpdateAsync(SessionRecord record);
    }
}
