using FluentValidation;

namespace TutorMatchingPlatform.Application.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
    {
        public UpdateSubjectCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("Subject Id is required.");

            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Subject Name is required.")
                .MaximumLength(50).WithMessage("Subject Name must not exceed 50 characters.");

            RuleFor(v => v.Description)
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
        }
    }
}
