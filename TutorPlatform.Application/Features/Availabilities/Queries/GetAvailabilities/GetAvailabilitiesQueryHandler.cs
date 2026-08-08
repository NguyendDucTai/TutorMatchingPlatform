using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Availabilities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Availabilities.Queries.GetAvailabilities
{
    public class GetAvailabilitiesQueryHandler : IRequestHandler<GetAvailabilitiesQuery, List<AvailabilityDto>>
    {
        private readonly IAvailabilityRepository _availabilityRepository;

        public GetAvailabilitiesQueryHandler(IAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
        }

        public async Task<List<AvailabilityDto>> Handle(GetAvailabilitiesQuery request, CancellationToken cancellationToken)
        {
            var availabilities = await _availabilityRepository.GetByUserIdAsync(request.TutorId);

            return availabilities.Select(a => new AvailabilityDto
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek.HasValue ? (int)a.DayOfWeek.Value : null,
                StartTime = a.StartTime.ToString(@"hh\:mm\:ss"),
                EndTime = a.EndTime.ToString(@"hh\:mm\:ss"),
                IsRecurring = a.IsRecurring,
                SpecificDate = a.SpecificDate
            }).ToList();
        }
    }
}
