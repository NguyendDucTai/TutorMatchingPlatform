using FluentValidation;

namespace TutorPlatform.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
    {
        public UpdateSubjectCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Subject ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Subject name cannot be empty.")
                .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.");
        }
    }
}
