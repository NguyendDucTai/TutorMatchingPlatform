using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Tutors;

namespace TutorMatchingPlatform.Application.Features.Tutors.Queries.GetTutorDetail
{
    public class GetTutorDetailQuery : IRequest<TutorSearchResultDto>
    {
        public Guid TutorUserId { get; set; }

        public GetTutorDetailQuery(Guid tutorUserId)
        {
            TutorUserId = tutorUserId;
        }
    }
}
