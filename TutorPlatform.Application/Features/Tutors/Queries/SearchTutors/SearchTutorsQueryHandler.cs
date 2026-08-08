using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Tutors;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQueryHandler : IRequestHandler<SearchTutorsQuery, PagedResult<TutorSearchResultDto>>
    {
        private readonly ITutorSearchRepository _tutorSearchRepository;

        public SearchTutorsQueryHandler(ITutorSearchRepository tutorSearchRepository)
        {
            _tutorSearchRepository = tutorSearchRepository;
        }

        public async Task<PagedResult<TutorSearchResultDto>> Handle(SearchTutorsQuery request, CancellationToken cancellationToken)
        {
            var pagedResult = await _tutorSearchRepository.SearchTutorsAsync(
                request.SubjectId,
                request.MinPrice,
                request.MaxPrice,
                request.DayOfWeek,
                request.SpecificDate,
                request.StartTime,
                request.EndTime,
                request.SearchQuery,
                request.SortBy,
                request.PageNumber,
                request.PageSize,
                request.MinRating
            );

            var dtos = pagedResult.Items.Select(x => new TutorSearchResultDto
            {
                TutorId = x.TutorId,
                FullName = x.FullName,
                AvatarUrl = x.AvatarUrl,
                Bio = x.Bio,
                Qualifications = x.Qualifications,
                AverageRating = x.AverageRating,
                TotalReviews = x.TotalReviews,
                TotalSessions = x.TotalSessions,
                Phone = x.Phone,
                Email = x.Email,
                DefaultMeetingLink = x.DefaultMeetingLink,
                Subjects = x.Subjects.Select(s => new TutorSearchSubjectResultDto
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    ProficiencyLevel = s.ProficiencyLevel,
                    HourlyCredits = s.HourlyCredits
                }).ToList()
            }).ToList();

            return new PagedResult<TutorSearchResultDto>(dtos, pagedResult.TotalCount);
        }
    }
}
