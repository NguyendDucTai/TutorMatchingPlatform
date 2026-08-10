namespace TutorPlatform.Domain.Enums
{
    public enum NotificationType
    {
        BookingCreated = 0,
        BookingConfirmed = 1,
        BookingCancelled = 2,
        BookingReminder = 3,
        ReviewReceived = 4,
        TutorApproved = 5,
        CreditChanged = 6,
        System = 7,
        TutorApprovalRequest = 8, // Sent to Admin when tutor submits profile
        TutorRejected = 9,         // Sent to Tutor when admin rejects
        OverdueClassWarning = 10,  // Sent to Tutor when booking time ends but tutor has not updated to Completed
        MeetingLinkUpdated = 11,   // Sent to Student when tutor updates meeting link
        BookingCompleted = 12      // Sent to Student when tutor completes booking
    }
}
