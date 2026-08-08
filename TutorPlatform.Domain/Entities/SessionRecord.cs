using System;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Entities
{
    public class SessionRecord : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public decimal? Score { get; private set; }
        public int CompletionPercentage { get; private set; }
        public string? TutorNotes { get; private set; }
        public string? Strengths { get; private set; }
        public string? AreasForImprovement { get; private set; }

        private SessionRecord() { } // EF Core

        public SessionRecord(Guid bookingId, decimal? score, int completionPercentage, string? tutorNotes, string? strengths, string? areasForImprovement)
        {
            if (completionPercentage < 0 || completionPercentage > 100) 
                throw new ArgumentException("CompletionPercentage must be between 0 and 100.");
            if (score.HasValue && (score < 0 || score > 100))
                throw new ArgumentException("Score must be between 0 and 100.");

            BookingId = bookingId;
            Score = score;
            CompletionPercentage = completionPercentage;
            TutorNotes = tutorNotes;
            Strengths = strengths;
            AreasForImprovement = areasForImprovement;
        }

        public void UpdateDetails(decimal? score, int completionPercentage, string? tutorNotes, string? strengths, string? areasForImprovement)
        {
            if (completionPercentage < 0 || completionPercentage > 100) 
                throw new ArgumentException("CompletionPercentage must be between 0 and 100.");
            if (score.HasValue && (score < 0 || score > 100))
                throw new ArgumentException("Score must be between 0 and 100.");

            Score = score;
            CompletionPercentage = completionPercentage;
            TutorNotes = tutorNotes;
            Strengths = strengths;
            AreasForImprovement = areasForImprovement;
            MarkUpdated();
        }
    }
}
