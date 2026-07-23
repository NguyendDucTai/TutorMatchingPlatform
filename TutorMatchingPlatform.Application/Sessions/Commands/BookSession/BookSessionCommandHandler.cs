using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Commands.BookSession
{
    public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, BookSessionResult>
    {
        private readonly IAppDbContext _context;

        public BookSessionCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<BookSessionResult> Handle(BookSessionCommand request, CancellationToken cancellationToken)
        {
            // 1. Get Student and Tutor data
            var studentUser = await _context.Users
                .Include(u => u.StudentProfile)
                .SingleOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

            var tutorUser = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.TutorId && u.Role == UserRole.Tutor, cancellationToken);

            if (studentUser == null || studentUser.StudentProfile == null || tutorUser == null || tutorUser.TutorProfile == null)
            {
                return new BookSessionResult { Success = false, Message = "Invalid student or tutor." };
            }

            // Ensure tutor is approved (BR-04)
            if (tutorUser.TutorProfile.Status != ProfileStatus.Approved)
            {
                return new BookSessionResult { Success = false, Message = "Tutor is not approved." };
            }

            // 2. Validate tutor teaches this subject (no payment logic)
            try
            {
                var subjects = JsonSerializer.Deserialize<List<SubjectRateEntry>>(tutorUser.TutorProfile.SubjectsJson ?? "[]") ?? new();
                var subjectMatch = subjects.FirstOrDefault(s => s.SubjectId == request.SubjectId);
                if (subjectMatch == null)
                {
                    return new BookSessionResult { Success = false, Message = "Tutor does not teach this subject." };
                }
            }
            catch
            {
                return new BookSessionResult { Success = false, Message = "Invalid tutor subjects configuration." };
            }

            // 3. Conflict Detection Job (BR-01, BR-02)
            // Check if there are any Confirmed/Pending sessions overlapping with the requested time 
            // for EITHER the Tutor OR the Student.
            var hasConflict = await _context.Sessions
                .AnyAsync(s => 
                    (s.TutorId == tutorUser.TutorProfile.Id || s.StudentId == studentUser.StudentProfile.Id) &&
                    (s.Status == SessionStatus.Confirmed || s.Status == SessionStatus.Pending || s.Status == SessionStatus.PendingChangeConfirmation) &&
                    s.StartTime < request.EndTime && 
                    s.EndTime > request.StartTime, 
                    cancellationToken);

            if (hasConflict)
            {
                return new BookSessionResult { Success = false, Message = "MSG04" };
            }

            // 4. Create Session
            var meetingLink = $"https://meet.google.com/mock-{Guid.NewGuid().ToString().Substring(0, 8)}";

            var session = new Session
            {
                TutorId = tutorUser.TutorProfile.Id,
                StudentId = studentUser.StudentProfile.Id,
                SubjectId = request.SubjectId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MeetingLink = meetingLink,
                Status = SessionStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return success (MSG03)
            return new BookSessionResult
            {
                Success = true,
                Message = "MSG03",
                SessionId = session.Id,
                MeetingLink = meetingLink
            };
        }

        private class SubjectRateEntry
        {
            public int SubjectId { get; set; }
            public decimal Rate { get; set; }
        }
    }
}
