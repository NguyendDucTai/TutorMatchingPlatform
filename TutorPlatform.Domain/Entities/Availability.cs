using System;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Entities
{
    public class Availability : BaseEntity
    {
        public Guid UserId { get; private set; }
        public DayOfWeek? DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public bool IsRecurring { get; private set; }
        public DateTime? SpecificDate { get; private set; }

        private Availability() { } // EF Core

        public Availability(Guid userId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime) throw new ArgumentException("StartTime must be before EndTime.");

            UserId = userId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            IsRecurring = true;
            SpecificDate = null;
        }

        public Availability(Guid userId, DateTime specificDate, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime) throw new ArgumentException("StartTime must be before EndTime.");

            UserId = userId;
            SpecificDate = specificDate.Date;
            StartTime = startTime;
            EndTime = endTime;
            IsRecurring = false;
            DayOfWeek = null;
        }
    }
}
