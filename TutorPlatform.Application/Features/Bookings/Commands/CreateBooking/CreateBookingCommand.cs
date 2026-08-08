using System;
using MediatR;

namespace TutorPlatform.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingResult
    {
        public Guid BookingId { get; set; }
        public bool Success { get; set; }
        public bool IsConflict { get; set; }
        public string? ErrorMessage { get; set; }

        public static CreateBookingResult Ok(Guid bookingId) => new CreateBookingResult { Success = true, BookingId = bookingId };
        public static CreateBookingResult Fail(string error, bool isConflict = false) => new CreateBookingResult { Success = false, ErrorMessage = error, IsConflict = isConflict };
    }

    public class CreateBookingCommand : IRequest<CreateBookingResult>
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
