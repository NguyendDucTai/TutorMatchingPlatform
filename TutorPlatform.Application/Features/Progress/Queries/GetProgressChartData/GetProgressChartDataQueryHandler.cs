using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Progress;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Progress.Queries.GetProgressChartData
{
    public class GetProgressChartDataQueryHandler : IRequestHandler<GetProgressChartDataQuery, ProgressChartDto>
    {
        private readonly ILearningGoalRepository _learningGoalRepository;
        private readonly ISessionRecordRepository _sessionRecordRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISubjectRepository _subjectRepository;

        public GetProgressChartDataQueryHandler(
            ILearningGoalRepository learningGoalRepository,
            ISessionRecordRepository sessionRecordRepository,
            IBookingRepository bookingRepository,
            ISubjectRepository subjectRepository)
        {
            _learningGoalRepository = learningGoalRepository;
            _sessionRecordRepository = sessionRecordRepository;
            _bookingRepository = bookingRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<ProgressChartDto> Handle(GetProgressChartDataQuery request, CancellationToken cancellationToken)
        {
            Guid targetStudentId;

            if (request.Role == UserRole.Student)
            {
                targetStudentId = request.CurrentUserId;
            }
            else if (request.Role == UserRole.Tutor)
            {
                if (!request.StudentId.HasValue)
                {
                    throw new BadRequestException("StudentId is required for tutors.");
                }
                targetStudentId = request.StudentId.Value;

                // Security check: Verify tutor teaches this student in this subject
                var studentBookings = await _bookingRepository.GetByStudentIdAsync(targetStudentId);
                var hasRelationship = studentBookings.Any(b =>
                    b.TutorId == request.CurrentUserId &&
                    b.SubjectId == request.SubjectId &&
                    (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed));

                if (!hasRelationship)
                {
                    throw new ForbiddenException("You are not authorized to view this student's progress in this subject.");
                }
            }
            else // Admin
            {
                if (!request.StudentId.HasValue)
                {
                    throw new BadRequestException("StudentId is required.");
                }
                targetStudentId = request.StudentId.Value;
            }

            // Verify Subject exists
            var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
            if (subject == null)
            {
                throw new NotFoundException(nameof(Subject), request.SubjectId);
            }

            // Fetch Learning Goals with progress history
            var goalsData = await _learningGoalRepository.GetByStudentIdAsync(targetStudentId);
            var subjectGoals = goalsData.Where(g => g.SubjectId == request.SubjectId).ToList();

            var today = DateTime.UtcNow.Date;

            var goalDtos = subjectGoals.Select(g =>
            {
                // Dynamic Overdue status mapping
                var status = (int)g.Status;
                if (g.Status != GoalStatus.Completed && g.TargetDate.HasValue && g.TargetDate.Value.Date < today)
                {
                    status = (int)GoalStatus.Overdue;
                }

                return new GoalChartDto
                {
                    GoalId = g.Id,
                    Title = g.Title,
                    Status = status,
                    ProgressHistory = g.GoalProgresses
                        .OrderBy(gp => gp.RecordedAt)
                        .Select(gp => new ProgressHistoryDto
                        {
                            RecordedAt = gp.RecordedAt,
                            ProgressPercentage = gp.ProgressPercentage
                        }).ToList()
                };
            }).ToList();

            // Fetch Session Records for completed bookings of this student and subject
            var sessionRecords = await _sessionRecordRepository.GetByStudentAndSubjectAsync(targetStudentId, request.SubjectId);
            
            // Map SessionRecords and order them by the associated booking's date
            // Note: Since MapToDomain loaded the SessionRecord, we also need to know the date of the booking.
            // We can get the booking list for this student, map them by bookingId, and fetch the ScheduledStartAt.
            var studentBookingsList = await _bookingRepository.GetByStudentIdAsync(targetStudentId);
            var bookingsMap = studentBookingsList.ToDictionary(b => b.Id, b => b.ScheduledStartAt);

            var sessionScores = sessionRecords
                .Where(sr => bookingsMap.ContainsKey(sr.BookingId))
                .Select(sr => new SessionScoreDto
                {
                    Date = bookingsMap[sr.BookingId],
                    Score = sr.Score ?? 0
                })
                .OrderBy(ss => ss.Date)
                .ToList();

            return new ProgressChartDto
            {
                StudentId = targetStudentId,
                SubjectId = request.SubjectId,
                SubjectName = subject.Name,
                Goals = goalDtos,
                SessionScores = sessionScores
            };
        }
    }
}
