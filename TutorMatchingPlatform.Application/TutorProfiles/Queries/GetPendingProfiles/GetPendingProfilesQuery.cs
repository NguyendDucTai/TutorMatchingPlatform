using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.TutorProfiles.DTOs;

namespace TutorMatchingPlatform.Application.TutorProfiles.Queries.GetPendingProfiles
{
    public class GetPendingProfilesQuery : IRequest<List<TutorProfileDto>>
    {
    }
}
