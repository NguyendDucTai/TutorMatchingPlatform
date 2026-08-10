using System;
using System.Collections.Generic;
using MediatR;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Application.Features.Bookings.Queries.GetMyBookings
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public int Status { get; set; }
        public string? MeetingLink { get; set; }
        public decimal CreditAmount { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TutorName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public bool IsStudentReviewed { get; set; }
        public bool IsTutorReviewed { get; set; }
        public int? StudentRating { get; set; }
        public string? StudentComment { get; set; }
        public int? TutorRating { get; set; }
        public string? TutorComment { get; set; }
        public string? CancellationReason { get; set; }
        public Guid? CancelledBy { get; set; }
    }

    public class GetMyBookingsQuery : IRequest<PagedResult<BookingDto>>
    {
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
