using System;

namespace TutorPlatform.Application.Contracts.Reviews
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = null!;
        public string? ReviewerAvatarUrl { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string ReviewType { get; set; } = null!; // "StudentToTutor" or "TutorToStudent"
        public string SubjectName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
