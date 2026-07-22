using FluentValidation;

namespace TutorMatchingPlatform.Application.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommandValidator : AbstractValidator<DeleteSubjectCommand>
    {
        public DeleteSubjectCommandValidator()
        {
            RuleFor(v => v.Id).GreaterThan(0);
        }
    }
}
