using System;
using System.Collections.Generic;
using MediatR;
using TutorPlatform.Application.Contracts.Availabilities;

namespace TutorPlatform.Application.Features.Availabilities.Commands.UpdateAvailability
{
    public class UpdateAvailabilityItemDto
    {
        public int? DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public DateTime? SpecificDate { get; set; }
    }

    public class UpdateAvailabilityCommand : IRequest<bool>
    {
        public Guid TutorId { get; set; }
        public List<UpdateAvailabilityItemDto> Availabilities { get; set; } = new List<UpdateAvailabilityItemDto>();
    }
}
