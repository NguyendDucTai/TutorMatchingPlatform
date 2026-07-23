namespace TutorMatchingPlatform.Application.Tutors.DTOs
{
    public class TutorDetailsDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public string? SubjectsJson { get; set; }
        public string? FreeSchedulesJson { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
