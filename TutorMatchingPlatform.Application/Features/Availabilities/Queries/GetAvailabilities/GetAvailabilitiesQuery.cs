using System;
using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Availabilities;

namespace TutorMatchingPlatform.Application.Features.Availabilities.Queries.GetAvailabilities
{
    public class GetAvailabilitiesQuery : IRequest<List<AvailabilityDto>>
    {
        public Guid TutorId { get; set; }
    }
}
