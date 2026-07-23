using System.Collections.Generic;

namespace TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors
{
    public class SearchTutorsResult
    {
        public bool HasResults { get; set; }
        public string? Message { get; set; }
        public List<TutorSearchResultDto> Tutors { get; set; } = new List<TutorSearchResultDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
