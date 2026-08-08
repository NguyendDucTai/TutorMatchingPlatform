using System;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public Guid ReviewerId { get; private set; }
        public Guid RevieweeId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public ReviewType ReviewType { get; private set; }

        private Review() { } // EF Core

        public Review(Guid bookingId, Guid reviewerId, Guid revieweeId, int rating, string? comment, ReviewType reviewType)
        {
            if (reviewerId == revieweeId) throw new ArgumentException("Reviewer and reviewee cannot be the same.");
            if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be between 1 and 5.");

            BookingId = bookingId;
            ReviewerId = reviewerId;
            RevieweeId = revieweeId;
            Rating = rating;
            Comment = comment;
            ReviewType = reviewType;
        }
    }
}
