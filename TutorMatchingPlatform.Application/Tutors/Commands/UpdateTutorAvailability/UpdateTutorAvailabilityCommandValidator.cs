using FluentValidation;

namespace TutorMatchingPlatform.Application.Tutors.Commands.UpdateTutorAvailability
{
    public class UpdateTutorAvailabilityCommandValidator : AbstractValidator<UpdateTutorAvailabilityCommand>
    {
        public UpdateTutorAvailabilityCommandValidator()
        {
            RuleFor(v => v.TimezoneOffset)
                .NotEmpty().WithMessage("TimezoneOffset is required.")
                .MaximumLength(10).WithMessage("TimezoneOffset must not exceed 10 characters.");
        }
    }
}
