using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class SessionRecordRepository : ISessionRecordRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SessionRecordRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SessionRecord?> GetByBookingIdAsync(Guid bookingId)
        {
            var dm = await _dbContext.SessionRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.BookingId == bookingId);

            if (dm == null) return null;

            return MapToDomain(dm);
        }

        public async Task<IReadOnlyList<SessionRecord>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId)
        {
            var dataModels = await _dbContext.SessionRecords
                .Include(sr => sr.Booking)
                .Where(sr => sr.Booking.StudentId == studentId && sr.Booking.SubjectId == subjectId)
                .AsNoTracking()
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<SessionRecord> AddAsync(SessionRecord record)
        {
            var dm = new SessionRecordDataModel
            {
                Id = record.Id,
                BookingId = record.BookingId,
                Score = record.Score,
                CompletionPercentage = record.CompletionPercentage,
                TutorNotes = record.TutorNotes,
                Strengths = record.Strengths,
                AreasForImprovement = record.AreasForImprovement,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };

            await _dbContext.SessionRecords.AddAsync(dm);
            await _dbContext.SaveChangesAsync();

            return record;
        }

        public async Task UpdateAsync(SessionRecord record)
        {
            var dm = await _dbContext.SessionRecords
                .FirstOrDefaultAsync(sr => sr.Id == record.Id);

            if (dm != null)
            {
                dm.Score = record.Score;
                dm.CompletionPercentage = record.CompletionPercentage;
                dm.TutorNotes = record.TutorNotes;
                dm.Strengths = record.Strengths;
                dm.AreasForImprovement = record.AreasForImprovement;
                dm.UpdatedAt = DateTime.UtcNow;

                _dbContext.SessionRecords.Update(dm);
                await _dbContext.SaveChangesAsync();
            }
        }

        private SessionRecord MapToDomain(SessionRecordDataModel dm)
        {
            var record = new SessionRecord(
                dm.BookingId,
                dm.Score,
                dm.CompletionPercentage,
                dm.TutorNotes,
                dm.Strengths,
                dm.AreasForImprovement
            );

            var type = typeof(SessionRecord);
            type.GetProperty("Id")?.SetValue(record, dm.Id);
            
            return record;
        }
    }
}
