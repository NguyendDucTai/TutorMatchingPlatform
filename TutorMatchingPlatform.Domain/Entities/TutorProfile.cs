using System;
using System.Collections.Generic;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class TutorProfile : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string? Bio { get; private set; }
        public string? Qualifications { get; private set; }
        public bool IsApproved { get; private set; }
        public int ApprovalStatus { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public Guid? ApprovedBy { get; private set; }
        public decimal AverageRating { get; private set; }
        public int TotalReviews { get; private set; }
        public int TotalSessions { get; private set; }
        public string? DefaultMeetingLink { get; private set; }

        private readonly List<TutorSubject> _tutorSubjects = new();
        public IReadOnlyCollection<TutorSubject> TutorSubjects => _tutorSubjects.AsReadOnly();

        private TutorProfile() { } // EF Core

        public TutorProfile(Guid userId, string? bio, string? qualifications)
        {
            UserId = userId;
            Bio = bio;
            Qualifications = qualifications;
            IsApproved = false;
            ApprovalStatus = 0;
            AverageRating = 0;
            TotalReviews = 0;
            TotalSessions = 0;
        }

        public void UpdateDetails(string? bio, string? qualifications)
        {
            Bio = bio;
            Qualifications = qualifications;
            if (ApprovalStatus == 2) // Resubmit on update
            {
                ApprovalStatus = 0;
                IsApproved = false;
            }
            MarkUpdated();
        }

        public void UpdateDefaultMeetingLink(string? link)
        {
            DefaultMeetingLink = link;
            MarkUpdated();
        }

        public void Approve(Guid adminUserId)
        {
            IsApproved = true;
            ApprovalStatus = 1;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = adminUserId;
            MarkUpdated();
        }

        public void Reject()
        {
            IsApproved = false;
            ApprovalStatus = 2;
            ApprovedAt = null;
            ApprovedBy = null;
            MarkUpdated();
        }

        public void RecalculateRating(int newRating)
        {
            decimal totalScore = (AverageRating * TotalReviews) + newRating;
            TotalReviews++;
            AverageRating = totalScore / TotalReviews;
            MarkUpdated();
        }

        public void IncrementSessions()
        {
            TotalSessions++;
            MarkUpdated();
        }

        public void AddTutorSubject(TutorSubject tutorSubject)
        {
            _tutorSubjects.Add(tutorSubject);
            MarkUpdated();
        }
        
        public void ClearTutorSubjects()
        {
            _tutorSubjects.Clear();
            MarkUpdated();
        }
    }
}
