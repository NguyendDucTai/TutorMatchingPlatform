using FluentValidation;

namespace TutorPlatform.Application.Features.Progress.Commands.CreateSessionRecord
{
    public class CreateSessionRecordCommandValidator : AbstractValidator<CreateSessionRecordCommand>
    {
        public CreateSessionRecordCommandValidator()
        {
            RuleFor(x => x.CompletionPercentage)
                .InclusiveBetween(0, 100).WithMessage("Completion percentage must be between 0 and 100.");

            RuleFor(x => x.Score)
                .Must(score => !score.HasValue || (score >= 0 && score <= 100))
                .WithMessage("Score must be between 0 and 100.");

            RuleFor(x => x.TutorNotes)
                .MaximumLength(2000).WithMessage("Tutor notes must not exceed 2000 characters.");

            RuleFor(x => x.Strengths)
                .MaximumLength(1000).WithMessage("Strengths text must not exceed 1000 characters.");

            RuleFor(x => x.AreasForImprovement)
                .MaximumLength(1000).WithMessage("Areas for improvement must not exceed 1000 characters.");
        }
    }
}
