using MediatR;
using TutorMatchingPlatform.Application.Users.DTOs;

namespace TutorMatchingPlatform.Application.Users.Queries.GetMyProfile
{
    public class GetMyProfileQuery : IRequest<UserProfileDto>
    {
        public int UserId { get; set; }
    }
}
