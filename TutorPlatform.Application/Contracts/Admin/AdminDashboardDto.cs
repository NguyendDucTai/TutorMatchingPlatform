using System;
using System.Collections.Generic;

namespace TutorPlatform.Application.Contracts.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalTutors { get; set; }
        public int TotalStudents { get; set; }
        public int PendingTutors { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double CompletionRate { get; set; }
        public List<AdminPopularSubjectDto> PopularSubjects { get; set; } = new();
        public double GoalCompletionRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public double TutorsGrowth { get; set; }
        public double StudentsGrowth { get; set; }
        public double BookingsGrowth { get; set; }
        public double RevenueGrowth { get; set; }
        public List<AdminRecentBookingDto> RecentBookings { get; set; } = new();
    }

    public class AdminPopularSubjectDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }

    public class AdminRecentBookingDto
    {
        public Guid BookingId { get; set; }
        public string TutorName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public decimal CreditAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
