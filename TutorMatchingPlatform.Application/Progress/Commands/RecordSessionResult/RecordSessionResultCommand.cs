using MediatR;

namespace TutorMatchingPlatform.Application.Progress.Commands.RecordSessionResult
{
    public class RecordSessionResultCommand : IRequest<RecordSessionResultResult>
    {
        public int TutorUserId { get; set; }  // From token claims
        public int SessionId { get; set; }

        // Optional score (0-10) and feedback (max 500 chars)
        public double? Score { get; set; }
        public string? TutorComment { get; set; }

        // Optional: Update an existing goal's completion %
        public int? MilestoneId { get; set; }
        public string? NewMilestoneName { get; set; }
        public int? CompletionPercentage { get; set; } // 0-100
    }
}
