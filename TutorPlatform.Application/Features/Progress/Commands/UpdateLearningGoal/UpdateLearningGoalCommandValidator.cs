using System;
using FluentValidation;

namespace TutorPlatform.Application.Features.Progress.Commands.UpdateLearningGoal
{
    public class UpdateLearningGoalCommandValidator : AbstractValidator<UpdateLearningGoalCommand>
    {
        public UpdateLearningGoalCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

            RuleFor(x => x.TargetDate)
                .Must(date => !date.HasValue || date.Value.Date >= DateTime.UtcNow.Date)
                .WithMessage("Target date must be in the future.");
        }
    }
}
