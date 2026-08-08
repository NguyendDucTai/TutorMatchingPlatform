using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class ReviewDataModel
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int ReviewType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public BookingDataModel Booking { get; set; } = null!;
        public UserDataModel Reviewer { get; set; } = null!;
        public UserDataModel Reviewee { get; set; } = null!;
    }
}
