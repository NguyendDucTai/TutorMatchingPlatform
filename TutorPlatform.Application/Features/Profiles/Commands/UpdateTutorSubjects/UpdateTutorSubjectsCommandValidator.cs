using FluentValidation;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorSubjects
{
    public class UpdateTutorSubjectsCommandValidator : AbstractValidator<UpdateTutorSubjectsCommand>
    {
        public UpdateTutorSubjectsCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleForEach(x => x.Subjects).ChildRules(subject => {
                subject.RuleFor(s => s.SubjectId).NotEmpty().WithMessage("SubjectId is required.");
                subject.RuleFor(s => s.ProficiencyLevel)
                    .InclusiveBetween(1, 5)
                    .WithMessage("ProficiencyLevel must be between 1 and 5.");
                subject.RuleFor(s => s.HourlyCredits)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("HourlyCredits must be zero or positive.");
            });
        }
    }
}
