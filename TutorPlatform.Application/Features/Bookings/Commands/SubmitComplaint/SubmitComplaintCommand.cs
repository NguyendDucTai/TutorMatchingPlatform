using System;
using MediatR;

namespace TutorPlatform.Application.Features.Bookings.Commands.SubmitComplaint
{
    public class SubmitComplaintCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
