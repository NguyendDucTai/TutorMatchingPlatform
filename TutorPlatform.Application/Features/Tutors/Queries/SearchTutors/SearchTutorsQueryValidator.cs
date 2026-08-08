using FluentValidation;

namespace TutorPlatform.Application.Features.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQueryValidator : AbstractValidator<SearchTutorsQuery>
    {
        public SearchTutorsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("PageSize must be greater than or equal to 1.");

            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0).WithMessage("MinPrice must be greater than or equal to 0.")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0).WithMessage("MaxPrice must be greater than or equal to 0.")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(x => x.MinPrice ?? 0).WithMessage("MaxPrice must be greater than or equal to MinPrice.")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("StartTime is required when EndTime is specified.")
                .When(x => x.EndTime.HasValue);

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("EndTime is required when StartTime is specified.")
                .When(x => x.StartTime.HasValue);

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime ?? TimeSpan.Zero).WithMessage("EndTime must be after StartTime.")
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
        }
    }
}
