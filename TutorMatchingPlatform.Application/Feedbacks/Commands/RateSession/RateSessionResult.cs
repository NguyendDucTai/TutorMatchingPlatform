namespace TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession
{
    public class RateSessionResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? FeedbackId { get; set; }
    }
}
