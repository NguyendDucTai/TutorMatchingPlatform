using FluentValidation;

namespace TutorMatchingPlatform.Application.Features.Profiles.Commands.UpdateStudentProfile
{
    public class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
    {
        public UpdateStudentProfileCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        }
    }
}
