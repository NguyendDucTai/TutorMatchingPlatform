using System;
using MediatR;

namespace TutorPlatform.Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
