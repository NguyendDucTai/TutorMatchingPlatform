namespace TutorMatchingPlatform.Application.Complaints.Commands.SubmitComplaint
{
    public class SubmitComplaintResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ComplaintId { get; set; }
    }
}
