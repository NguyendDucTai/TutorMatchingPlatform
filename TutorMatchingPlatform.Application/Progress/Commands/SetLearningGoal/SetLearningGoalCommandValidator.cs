using FluentValidation;

namespace TutorMatchingPlatform.Application.Progress.Commands.SetLearningGoal
{
    public class SetLearningGoalCommandValidator : AbstractValidator<SetLearningGoalCommand>
    {
        public SetLearningGoalCommandValidator()
        {
            RuleFor(v => v.StudentId).GreaterThan(0).WithMessage("StudentId is required.");
            RuleFor(v => v.SubjectId).GreaterThan(0).WithMessage("SubjectId is required.");
            
            RuleFor(v => v.MilestoneName)
                .NotEmpty().WithMessage("MilestoneName is required.")
                .MaximumLength(100).WithMessage("MilestoneName cannot exceed 100 characters.");
        }
    }
}
