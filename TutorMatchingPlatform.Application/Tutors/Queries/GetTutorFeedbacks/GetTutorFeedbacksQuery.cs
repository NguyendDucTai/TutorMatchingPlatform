using MediatR;
using TutorMatchingPlatform.Application.Tutors.DTOs;

namespace TutorMatchingPlatform.Application.Tutors.Queries.GetTutorFeedbacks
{
    public class GetTutorFeedbacksQuery : IRequest<PaginatedTutorFeedbacksDto>
    {
        public int TutorId { get; set; } // This is UserId of the Tutor
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
