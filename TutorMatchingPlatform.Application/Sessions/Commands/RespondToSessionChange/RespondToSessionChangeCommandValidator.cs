using FluentValidation;

namespace TutorMatchingPlatform.Application.Sessions.Commands.RespondToSessionChange
{
    public class RespondToSessionChangeCommandValidator : AbstractValidator<RespondToSessionChangeCommand>
    {
        public RespondToSessionChangeCommandValidator()
        {
            RuleFor(v => v.ChangeRequestId).GreaterThan(0);
        }
    }
}
