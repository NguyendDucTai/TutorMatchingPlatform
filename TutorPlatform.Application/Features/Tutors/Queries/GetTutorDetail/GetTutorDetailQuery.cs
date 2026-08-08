using System;
using MediatR;
using TutorPlatform.Application.Contracts.Tutors;

namespace TutorPlatform.Application.Features.Tutors.Queries.GetTutorDetail
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
