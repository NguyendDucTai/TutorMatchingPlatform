using MediatR;

namespace TutorMatchingPlatform.Application.Sessions.Queries.GetSessionJoinLink
{
    public class GetSessionJoinLinkQuery : IRequest<SessionJoinLinkResult>
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
    }

    public class SessionJoinLinkResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MeetingLink { get; set; }
        public bool CanJoin { get; set; }
        public bool PromptTutorToAddLink { get; set; }
    }
}
