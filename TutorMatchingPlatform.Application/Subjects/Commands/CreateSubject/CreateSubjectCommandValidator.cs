using FluentValidation;

namespace TutorMatchingPlatform.Application.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
    {
        public CreateSubjectCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Subject Name is required.")
                .MaximumLength(50).WithMessage("Subject Name must not exceed 50 characters.");

            RuleFor(v => v.Description)
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
        }
    }
}
