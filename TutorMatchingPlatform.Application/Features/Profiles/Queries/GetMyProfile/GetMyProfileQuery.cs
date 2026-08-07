using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Profiles;

namespace TutorMatchingPlatform.Application.Features.Profiles.Queries.GetMyProfile
{
    public class GetMyProfileQuery : IRequest<MyProfileResponse>
    {
        public Guid UserId { get; set; }
    }
}
