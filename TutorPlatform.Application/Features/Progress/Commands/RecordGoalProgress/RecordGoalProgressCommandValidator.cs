using FluentValidation;

namespace TutorPlatform.Application.Features.Progress.Commands.RecordGoalProgress
{
    public class RecordGoalProgressCommandValidator : AbstractValidator<RecordGoalProgressCommand>
    {
        public RecordGoalProgressCommandValidator()
        {
            RuleFor(x => x.ProgressPercentage)
                .InclusiveBetween(0, 100).WithMessage("Progress percentage must be between 0 and 100.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
        }
    }
}
