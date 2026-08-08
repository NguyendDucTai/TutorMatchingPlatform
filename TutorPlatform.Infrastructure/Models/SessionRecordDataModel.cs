using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class SessionRecordDataModel
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal? Score { get; set; }
        public int CompletionPercentage { get; set; }
        public string? TutorNotes { get; set; }
        public string? Strengths { get; set; }
        public string? AreasForImprovement { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public BookingDataModel Booking { get; set; } = null!;
    }
}
