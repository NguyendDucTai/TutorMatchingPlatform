namespace TutorMatchingPlatform.Application.Sessions.Commands.BookSession
{
    public class BookSessionResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; } // MSG03, MSG04, MSG14
        public int? SessionId { get; set; }
        public string? MeetingLink { get; set; }
    }
}
