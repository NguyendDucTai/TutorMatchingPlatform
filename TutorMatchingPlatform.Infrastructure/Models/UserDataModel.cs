using System;
using System.Collections.Generic;

namespace TutorMatchingPlatform.Infrastructure.Models
{
    public class UserDataModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public int Role { get; set; }
        public bool IsActive { get; set; }
        public decimal CreditBalance { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public TutorProfileDataModel? TutorProfile { get; set; }
        public StudentProfileDataModel? StudentProfile { get; set; }
        public ICollection<AvailabilityDataModel> Availabilities { get; set; } = new List<AvailabilityDataModel>();
        public ICollection<NotificationDataModel> Notifications { get; set; } = new List<NotificationDataModel>();
        public ICollection<CreditTransactionDataModel> CreditTransactions { get; set; } = new List<CreditTransactionDataModel>();
        public ICollection<BookingDataModel> BookingsAsTutor { get; set; } = new List<BookingDataModel>();
        public ICollection<BookingDataModel> BookingsAsStudent { get; set; } = new List<BookingDataModel>();
    }
}
