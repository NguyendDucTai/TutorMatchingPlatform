using FluentValidation;

namespace TutorPlatform.Application.Features.Credits.Commands.DepositCredits
{
    public class DepositCreditsCommandValidator : AbstractValidator<DepositCreditsCommand>
    {
        public DepositCreditsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Deposit amount must be greater than zero.")
                .LessThanOrEqualTo(10000000).WithMessage("Deposit amount must not exceed 10,000,000 credits per transaction.");
        }
    }
}
