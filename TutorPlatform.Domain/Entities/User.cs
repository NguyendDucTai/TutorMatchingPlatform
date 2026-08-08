using System;
using System.Collections.Generic;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string FullName { get; private set; }
        public string? Phone { get; private set; }
        public string? AvatarUrl { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public decimal CreditBalance { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }
        public string? AdminNote { get; private set; }

        private readonly List<Availability> _availabilities = new();
        public IReadOnlyCollection<Availability> Availabilities => _availabilities.AsReadOnly();

        private readonly List<Notification> _notifications = new();
        public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

        private readonly List<CreditTransaction> _creditTransactions = new();
        public IReadOnlyCollection<CreditTransaction> CreditTransactions => _creditTransactions.AsReadOnly();

        private User() { } // EF Core

        public User(string email, string passwordHash, string fullName, string? phone, string? avatarUrl, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.");
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName cannot be empty.");

            Email = email;
            PasswordHash = passwordHash;
            FullName = fullName;
            Phone = phone;
            AvatarUrl = avatarUrl;
            Role = role;
            IsActive = true;
            CreditBalance = 0;
        }

        public void UpdateProfile(string fullName, string? phone, string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName cannot be empty.");
            FullName = fullName;
            Phone = phone;
            AvatarUrl = avatarUrl;
            MarkUpdated();
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash cannot be empty.");
            PasswordHash = newPasswordHash;
            MarkUpdated();
        }

        public void SetRefreshToken(string token, DateTime expiryTime)
        {
            RefreshToken = token;
            RefreshTokenExpiryTime = expiryTime;
            MarkUpdated();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkUpdated();
        }

        public void Activate()
        {
            IsActive = true;
            MarkUpdated();
        }

        public void AdjustCredits(decimal amount)
        {
            CreditBalance += amount;
            MarkUpdated();
        }

        public void UpdateAdminNote(string? note)
        {
            AdminNote = note;
            MarkUpdated();
        }
    }
}
