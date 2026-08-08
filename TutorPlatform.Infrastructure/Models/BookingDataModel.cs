using System;
using System.Collections.Generic;

namespace TutorPlatform.Infrastructure.Models
{
    public class BookingDataModel
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public int Status { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
        public decimal CreditAmount { get; set; }
        public string? CancellationReason { get; set; }
        public Guid? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel Tutor { get; set; } = null!;
        public UserDataModel Student { get; set; } = null!;
        public SubjectDataModel Subject { get; set; } = null!;
        public SessionRecordDataModel? SessionRecord { get; set; }
        public ICollection<ReviewDataModel> Reviews { get; set; } = new List<ReviewDataModel>();
        public ICollection<CreditTransactionDataModel> CreditTransactions { get; set; } = new List<CreditTransactionDataModel>();
    }
}
