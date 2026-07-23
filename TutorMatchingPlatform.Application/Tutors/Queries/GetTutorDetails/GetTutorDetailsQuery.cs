using MediatR;
using TutorMatchingPlatform.Application.Tutors.DTOs;

namespace TutorMatchingPlatform.Application.Tutors.Queries.GetTutorDetails
{
    public class GetTutorDetailsQuery : IRequest<TutorDetailsDto>
    {
        public int TutorId { get; set; } // This is UserId of the Tutor
    }
}
