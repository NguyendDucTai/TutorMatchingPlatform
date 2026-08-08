using System;
using MediatR;

namespace TutorPlatform.Application.Features.Progress.Commands.CreateSessionRecord
{
    public class CreateSessionRecordCommand : IRequest<Guid>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid BookingId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid TutorId { get; set; }
        public decimal? Score { get; set; }
        public int CompletionPercentage { get; set; }
        public string? TutorNotes { get; set; }
        public string? Strengths { get; set; }
        public string? AreasForImprovement { get; set; }
    }
}
