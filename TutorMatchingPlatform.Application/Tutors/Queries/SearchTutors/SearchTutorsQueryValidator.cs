using FluentValidation;

namespace TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQueryValidator : AbstractValidator<SearchTutorsQuery>
    {
        public SearchTutorsQueryValidator()
        {
            RuleFor(v => v.SubjectId)
                .GreaterThan(0).WithMessage("SubjectId is required and must be a positive number.");

            RuleFor(v => v.MinRate)
                .GreaterThan(0).When(v => v.MinRate.HasValue)
                .WithMessage("MinRate must be greater than 0.");

            RuleFor(v => v.MaxRate)
                .GreaterThan(0).When(v => v.MaxRate.HasValue)
                .WithMessage("MaxRate must be greater than 0.")
                .GreaterThanOrEqualTo(v => v.MinRate)
                .When(v => v.MinRate.HasValue && v.MaxRate.HasValue)
                .WithMessage("MaxRate must be greater than or equal to MinRate.");

            RuleFor(v => v.PageSize)
                .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");

            RuleFor(v => v.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        }
    }
}
