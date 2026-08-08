using FluentValidation;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommandValidator : AbstractValidator<UpdateTutorProfileCommand>
    {
        public UpdateTutorProfileCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        }
    }
}
