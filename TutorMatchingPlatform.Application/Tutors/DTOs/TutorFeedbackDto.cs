using System;

namespace TutorMatchingPlatform.Application.Tutors.DTOs
{
    public class TutorFeedbackDto
    {
        public int FeedbackId { get; set; }
        public int SessionId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaginatedTutorFeedbacksDto
    {
        public int TotalCount { get; set; }
        public System.Collections.Generic.List<TutorFeedbackDto> Feedbacks { get; set; } = new();
    }
}
