using System;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid TutorId { get; private set; }
        public Guid StudentId { get; private set; }
        public Guid SubjectId { get; private set; }
        public DateTime ScheduledStartAt { get; private set; }
        public DateTime ScheduledEndAt { get; private set; }
        public BookingStatus Status { get; private set; }
        public string? MeetingLink { get; private set; }
        public string? Notes { get; private set; }
        public decimal CreditAmount { get; private set; }
        public string? CancellationReason { get; private set; }
        public Guid? CancelledBy { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public SessionRecord? SessionRecord { get; private set; }

        private Booking() { } // EF Core

        public Booking(Guid tutorId, Guid studentId, Guid subjectId, DateTime startAt, DateTime endAt, decimal creditAmount, string? notes)
        {
            if (tutorId == studentId) throw new ArgumentException("Tutor and student cannot be the same.");
            if (startAt >= endAt) throw new ArgumentException("ScheduledStartAt must be before ScheduledEndAt.");

            TutorId = tutorId;
            StudentId = studentId;
            SubjectId = subjectId;
            ScheduledStartAt = startAt;
            ScheduledEndAt = endAt;
            CreditAmount = creditAmount;
            Notes = notes;
            Status = BookingStatus.Pending;
        }

        // Constructor with explicit ID (used when ID must be known before persisting)
        public Booking(Guid id, Guid tutorId, Guid studentId, Guid subjectId, DateTime startAt, DateTime endAt, decimal creditAmount, string? notes)
            : this(tutorId, studentId, subjectId, startAt, endAt, creditAmount, notes)
        {
            Id = id;
        }

        public void Confirm(string? meetingLink)
        {
            if (Status != BookingStatus.Pending) throw new InvalidOperationException("Only pending bookings can be confirmed.");
            Status = BookingStatus.Confirmed;
            MeetingLink = meetingLink;
            MarkUpdated();
        }

        public void UpdateMeetingLink(string? meetingLink)
        {
            if (Status != BookingStatus.Confirmed) throw new InvalidOperationException("Only confirmed bookings can update meeting link.");
            MeetingLink = meetingLink;
            MarkUpdated();
        }

        public void Complete()
        {
            if (Status != BookingStatus.Confirmed) throw new InvalidOperationException("Only confirmed bookings can be completed.");
            
            Status = BookingStatus.Completed;
            MarkUpdated();
        }

        public void Cancel(Guid cancelledBy, string reason)
        {
            if (Status == BookingStatus.Completed || Status == BookingStatus.Cancelled) 
                throw new InvalidOperationException("Cannot cancel a completed or already cancelled booking.");

            Status = BookingStatus.Cancelled;
            CancelledBy = cancelledBy;
            CancellationReason = reason;
            CancelledAt = DateTime.UtcNow;
            MarkUpdated();
        }

        public void Reschedule(DateTime newStart, DateTime newEnd, string reason)
        {
            if (Status == BookingStatus.Completed || Status == BookingStatus.Cancelled) 
                throw new InvalidOperationException("Cannot reschedule a completed or cancelled booking.");
            if (newStart >= newEnd) throw new ArgumentException("ScheduledStartAt must be before ScheduledEndAt.");

            ScheduledStartAt = newStart;
            ScheduledEndAt = newEnd;
            Status = BookingStatus.Rescheduled;
            Notes = $"{Notes}\nRescheduled reason: {reason}";
            MarkUpdated();
        }
    }
}
