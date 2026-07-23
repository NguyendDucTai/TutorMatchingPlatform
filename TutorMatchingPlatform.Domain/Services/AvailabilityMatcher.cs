using System;
using System.Collections.Generic;

namespace TutorMatchingPlatform.Domain.Services
{
    public class TimeSlot
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty; // Format: "HH:mm"
        public string EndTime { get; set; } = string.Empty;   // Format: "HH:mm"
    }

    public static class AvailabilityMatcher
    {

        public static int CalculateOverlapMinutes(List<TimeSlot> tutorSlots, List<TimeSlot> studentSlots)
        {
            int totalOverlapMinutes = 0;

            foreach (var tutorSlot in tutorSlots)
            {
                foreach (var studentSlot in studentSlots)
                {
                    // Must be the same day of week
                    if (!string.Equals(tutorSlot.DayOfWeek, studentSlot.DayOfWeek, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Parse times
                    if (!TimeSpan.TryParse(tutorSlot.StartTime, out var tutorStart) ||
                        !TimeSpan.TryParse(tutorSlot.EndTime, out var tutorEnd) ||
                        !TimeSpan.TryParse(studentSlot.StartTime, out var studentStart) ||
                        !TimeSpan.TryParse(studentSlot.EndTime, out var studentEnd))
                        continue;

                    // Calculate overlap
                    var overlapStart = tutorStart > studentStart ? tutorStart : studentStart;
                    var overlapEnd = tutorEnd < studentEnd ? tutorEnd : studentEnd;

                    if (overlapEnd > overlapStart)
                    {
                        totalOverlapMinutes += (int)(overlapEnd - overlapStart).TotalMinutes;
                    }
                }
            }

            return totalOverlapMinutes;
        }
    }
}
