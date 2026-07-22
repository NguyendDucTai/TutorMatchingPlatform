using FluentValidation;

namespace TutorMatchingPlatform.Application.Progress.Commands.RecordSessionResult
{
    public class RecordSessionResultCommandValidator : AbstractValidator<RecordSessionResultCommand>
    {
        public RecordSessionResultCommandValidator()
        {
            RuleFor(v => v.SessionId).GreaterThan(0).WithMessage("SessionId is required.");

            // Score optional, but if provided must be 0-10
            RuleFor(v => v.Score)
                .InclusiveBetween(0, 10)
                .When(v => v.Score.HasValue)
                .WithMessage("Score must be between 0 and 10.");

            // TutorComment max 500 chars
            RuleFor(v => v.TutorComment)
                .MaximumLength(500)
                .When(v => v.TutorComment != null)
                .WithMessage("Feedback cannot exceed 500 characters.");

            // If MilestoneId is provided, CompletionPercentage must be in range 0-100
            RuleFor(v => v.CompletionPercentage)
                .InclusiveBetween(0, 100)
                .When(v => v.MilestoneId.HasValue)
                .WithMessage("CompletionPercentage must be between 0 and 100.");
        }
    }
}
