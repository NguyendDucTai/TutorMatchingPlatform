using FluentValidation;

namespace TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession
{
    public class RateSessionCommandValidator : AbstractValidator<RateSessionCommand>
    {
        public RateSessionCommandValidator()
        {
            RuleFor(v => v.SessionId).GreaterThan(0).WithMessage("SessionId is required.");

            RuleFor(v => v.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5 stars.");

            RuleFor(v => v.Comment)
                .MaximumLength(300)
                .When(v => v.Comment != null)
                .WithMessage("Comment cannot exceed 300 characters.");
        }
    }
}
