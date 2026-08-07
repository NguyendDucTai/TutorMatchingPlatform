using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<Guid>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public Guid SubjectId { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public string? Notes { get; set; }
    }
}
