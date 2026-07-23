using MediatR;

namespace TutorMatchingPlatform.Application.Tutors.Commands.UpdateTutorAvailability
{
    public class UpdateTutorAvailabilityCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? FreeSchedulesJson { get; set; }
        public string? TimezoneOffset { get; set; }
    }
}
