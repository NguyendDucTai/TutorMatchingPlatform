using MediatR;
using System;

namespace TutorMatchingPlatform.Application.Sessions.Commands.UpdateMeetingLink
{
    public class UpdateMeetingLinkCommand : IRequest<UpdateMeetingLinkResult>
    {
        public int SessionId { get; set; }
        public int TutorUserId { get; set; }
        public string MeetingLink { get; set; } = string.Empty;
    }

    public class UpdateMeetingLinkResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
