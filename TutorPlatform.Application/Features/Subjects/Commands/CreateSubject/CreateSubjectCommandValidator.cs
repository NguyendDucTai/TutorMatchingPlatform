using FluentValidation;

namespace TutorPlatform.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
    {
        public CreateSubjectCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Subject name is required.");
        }
    }
}
