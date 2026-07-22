namespace TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange
{
    public class ProposeSessionChangeResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ChangeRequestId { get; set; }
        public bool IsLateCancellation { get; set; }
    }
}
