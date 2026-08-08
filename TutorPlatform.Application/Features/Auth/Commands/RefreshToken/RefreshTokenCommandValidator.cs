using FluentValidation;

namespace TutorPlatform.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty().WithMessage("AccessToken is required.");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("RefreshToken is required.");
        }
    }
}
