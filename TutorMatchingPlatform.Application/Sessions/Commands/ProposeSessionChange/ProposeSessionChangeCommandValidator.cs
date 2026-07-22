using FluentValidation;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange
{
    public class ProposeSessionChangeCommandValidator : AbstractValidator<ProposeSessionChangeCommand>
    {
        public ProposeSessionChangeCommandValidator()
        {
            RuleFor(v => v.SessionId).GreaterThan(0);
            
            RuleFor(v => v.ChangeType).IsInEnum();

            RuleFor(v => v.NewStartTime)
                .NotEmpty()
                .When(v => v.ChangeType == SessionChangeType.Reschedule)
                .WithMessage("NewStartTime is required for Reschedule.");

            RuleFor(v => v.NewEndTime)
                .NotEmpty()
                .When(v => v.ChangeType == SessionChangeType.Reschedule)
                .WithMessage("NewEndTime is required for Reschedule.");
                
            RuleFor(v => v.NewEndTime)
                .GreaterThan(v => v.NewStartTime)
                .When(v => v.ChangeType == SessionChangeType.Reschedule && v.NewStartTime.HasValue)
                .WithMessage("NewEndTime must be after NewStartTime.");
        }
    }
}
