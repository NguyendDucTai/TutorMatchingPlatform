using System;
using System.Collections.Generic;
using MediatR;
using TutorPlatform.Application.Contracts.Availabilities;

namespace TutorPlatform.Application.Features.Availabilities.Queries.GetAvailabilities
{
    public class GetAvailabilitiesQuery : IRequest<List<AvailabilityDto>>
    {
        public Guid TutorId { get; set; }
    }
}
