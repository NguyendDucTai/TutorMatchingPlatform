using System;
using MediatR;

namespace TutorPlatform.Application.Features.Bookings.Commands.ConfirmBooking
{
    public class ConfirmBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid TutorId { get; set; }
        public string? MeetingLink { get; set; }
    }
}
