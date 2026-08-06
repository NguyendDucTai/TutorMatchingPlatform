using System;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Availability : BaseEntity
    {
        public int TutorProfileId { get; set; }
        public TutorProfile TutorProfile { get; set; } = null!;
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsRecurring { get; set; } = true;
        public DateTime? SpecificDate { get; set; }
    }
}
