using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Bookings.Commands.UpdateMeetingLink
{
    public class UpdateMeetingLinkCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid TutorId { get; set; }
        public string? MeetingLink { get; set; }
    }
}
