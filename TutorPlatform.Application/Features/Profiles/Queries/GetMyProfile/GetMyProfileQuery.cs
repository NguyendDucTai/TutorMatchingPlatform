using System;
using MediatR;
using TutorPlatform.Application.Contracts.Profiles;

namespace TutorPlatform.Application.Features.Profiles.Queries.GetMyProfile
{
    public class GetMyProfileQuery : IRequest<MyProfileResponse>
    {
        public Guid UserId { get; set; }
    }
}
