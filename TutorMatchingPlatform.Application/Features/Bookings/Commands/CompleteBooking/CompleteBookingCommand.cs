using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid TutorId { get; set; }
    }
}
