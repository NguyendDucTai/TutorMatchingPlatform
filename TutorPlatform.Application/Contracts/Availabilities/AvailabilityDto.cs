using System;

namespace TutorPlatform.Application.Contracts.Availabilities
{
    public class AvailabilityDto
    {
        public Guid Id { get; set; }
        public int? DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public DateTime? SpecificDate { get; set; }
    }
}
