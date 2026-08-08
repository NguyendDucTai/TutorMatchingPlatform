using System;
using FluentValidation;

namespace TutorPlatform.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.TutorId).NotEmpty().WithMessage("TutorId is required.");
            RuleFor(x => x.SubjectId).NotEmpty().WithMessage("SubjectId is required.");
            
            RuleFor(x => x.ScheduledStartAt)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

            RuleFor(x => x.ScheduledEndAt)
                .NotEmpty()
                .GreaterThan(x => x.ScheduledStartAt).WithMessage("End time must be after start time.");
        }
    }
}
