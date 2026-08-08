using System;
using MediatR;

namespace TutorPlatform.Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid TutorId { get; set; }
    }
}
