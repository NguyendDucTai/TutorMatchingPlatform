using FluentValidation;

namespace TutorMatchingPlatform.Application.Credits.Commands.DepositCredit
{
    public class DepositCreditCommandValidator : AbstractValidator<DepositCreditCommand>
    {
        public DepositCreditCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Deposit amount must be greater than zero.");
        }
    }
}
