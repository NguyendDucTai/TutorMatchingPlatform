using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Application.Contracts.Tutors;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Tutors.Queries.GetTutorDetail
{
    public class GetTutorDetailQueryHandler : IRequestHandler<GetTutorDetailQuery, TutorSearchResultDto>
    {
        private readonly ITutorSearchRepository _tutorSearchRepository;

        public GetTutorDetailQueryHandler(ITutorSearchRepository tutorSearchRepository)
        {
            _tutorSearchRepository = tutorSearchRepository;
        }

        public async Task<TutorSearchResultDto> Handle(GetTutorDetailQuery request, CancellationToken cancellationToken)
        {
            var result = await _tutorSearchRepository.GetTutorDetailAsync(request.TutorUserId);
            
            if (result == null)
            {
                throw new NotFoundException("TutorProfile", request.TutorUserId);
            }

            return new TutorSearchResultDto
            {
                TutorId = result.TutorId,
                FullName = result.FullName,
                AvatarUrl = result.AvatarUrl,
                Bio = result.Bio,
                Qualifications = result.Qualifications,
                AverageRating = result.AverageRating,
                TotalReviews = result.TotalReviews,
                TotalSessions = result.TotalSessions,
                Phone = result.Phone,
                Email = result.Email,
                DefaultMeetingLink = result.DefaultMeetingLink,
                Subjects = result.Subjects.Select(s => new TutorSearchSubjectResultDto
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    ProficiencyLevel = s.ProficiencyLevel,
                    HourlyCredits = s.HourlyCredits
                }).ToList()
            };
        }
    }
}
