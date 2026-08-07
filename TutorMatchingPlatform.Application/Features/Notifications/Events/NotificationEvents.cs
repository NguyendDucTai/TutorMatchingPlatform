using MediatR;
using System;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Features.Notifications.Events
{
    public class BookingCreatedEvent : INotification
    {
        public Booking Booking { get; set; } = null!;
        public string StudentName { get; set; } = string.Empty;
    }

    public class BookingCancelledEvent : INotification
    {
        public Booking Booking { get; set; } = null!;
        public string CancelledByUserName { get; set; } = string.Empty;
        public Guid CancelledByUserId { get; set; }
    }

    public class BookingConfirmedEvent : INotification
    {
        public Booking Booking { get; set; } = null!;
        public string TutorName { get; set; } = string.Empty;
    }

    public class ReviewReceivedEvent : INotification
    {
        public Review Review { get; set; } = null!;
        public string ReviewerName { get; set; } = string.Empty;
    }

    public class CreditChangedEvent : INotification
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class DepositRequestCreatedEvent : INotification
    {
        public Guid RequestId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DepositRequestApprovedEvent : INotification
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
    }
}
