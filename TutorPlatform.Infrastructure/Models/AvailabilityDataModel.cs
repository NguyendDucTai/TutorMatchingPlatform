using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class AvailabilityDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? SpecificDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
    }
}
