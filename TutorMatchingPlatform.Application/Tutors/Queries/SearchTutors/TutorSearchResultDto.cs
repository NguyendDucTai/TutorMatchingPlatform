namespace TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors
{
    public class TutorSearchResultDto
    {
        public int TutorProfileId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? SubjectsJson { get; set; }
        public double ReputationScore { get; set; }
        public int OverlapMinutes { get; set; }
        public double RankingScore { get; set; }
    }
}
