using System;
using FluentValidation;

namespace TutorPlatform.Application.Features.Availabilities.Commands.UpdateAvailability
{
    public class UpdateAvailabilityCommandValidator : AbstractValidator<UpdateAvailabilityCommand>
    {
        public UpdateAvailabilityCommandValidator()
        {
            RuleFor(x => x.TutorId).NotEmpty().WithMessage("TutorId is required.");
            RuleForEach(x => x.Availabilities).ChildRules(availability => {
                availability.RuleFor(a => a.StartTime)
                    .NotEmpty()
                    .Must(BeAValidTime).WithMessage("StartTime must be a valid time format (HH:mm:ss).");
                
                availability.RuleFor(a => a.EndTime)
                    .NotEmpty()
                    .Must(BeAValidTime).WithMessage("EndTime must be a valid time format (HH:mm:ss).");

                availability.RuleFor(a => a)
                    .Must(a => IsEndTimeAfterStartTime(a.StartTime, a.EndTime))
                    .WithMessage("EndTime must be after StartTime.");

                availability.RuleFor(a => a)
                    .Must(a => !a.IsRecurring || (a.DayOfWeek.HasValue && a.DayOfWeek >= 0 && a.DayOfWeek <= 6))
                    .WithMessage("DayOfWeek must be between 0 and 6 for recurring availabilities.");

                availability.RuleFor(a => a)
                    .Must(a => a.IsRecurring || a.SpecificDate.HasValue)
                    .WithMessage("SpecificDate must be provided for non-recurring availabilities.");
            });
        }

        private bool BeAValidTime(string timeString)
        {
            return TimeSpan.TryParse(timeString, out _);
        }

        private bool IsEndTimeAfterStartTime(string start, string end)
        {
            if (TimeSpan.TryParse(start, out var startTime) && TimeSpan.TryParse(end, out var endTime))
            {
                return endTime > startTime;
            }
            return false;
        }
    }
}
