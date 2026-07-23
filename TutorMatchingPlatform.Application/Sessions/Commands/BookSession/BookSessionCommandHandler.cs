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
        private readonly IEmailService _emailService;

        public BookSessionCommandHandler(IAppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

            // 2. Validate tutor teaches this subject and process payment
            decimal sessionFee = 0;
            try
            {
                var subjects = JsonSerializer.Deserialize<List<SubjectRateEntry>>(tutorUser.TutorProfile.SubjectsJson ?? "[]") ?? new();
                var subjectMatch = subjects.FirstOrDefault(s => s.SubjectId == request.SubjectId);
                if (subjectMatch == null)
                {
                    return new BookSessionResult { Success = false, Message = "Tutor does not teach this subject." };
                }
                sessionFee = subjectMatch.Rate;
            }
            catch
            {
                return new BookSessionResult { Success = false, Message = "Invalid tutor subjects configuration." };
            }

            if (studentUser.CreditBalance < sessionFee)
            {
                return new BookSessionResult { Success = false, Message = "Insufficient credits to book this session." };
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

            // Deduct credits from student
            studentUser.CreditBalance -= sessionFee;

            var transaction = new CreditTransaction
            {
                UserId = studentUser.Id,
                Amount = -sessionFee,
                Type = CreditTransactionType.SessionFee,
                Description = "Paid session fee for booking",
                CreatedAt = DateTime.UtcNow
                // ReferenceId will be set after session gets its Id
            };

            _context.Sessions.Add(session);
            _context.CreditTransactions.Add(transaction);
            
            await _context.SaveChangesAsync(cancellationToken);

            transaction.ReferenceId = session.Id.ToString();
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return success (MSG03)
            await _emailService.SendEmailAsync(studentUser.Email, "Booking Confirmed", $"Your session for subject {request.SubjectId} is confirmed. MSG03");
            await _emailService.SendEmailAsync(tutorUser.Email, "New Booking", $"You have a new booking from {studentUser.FullName}. MSG03");

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
