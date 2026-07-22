using MediatR;

namespace TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQuery : IRequest<SearchTutorsResult>
    {
        public int SubjectId { get; set; }             
        public string? StudentScheduleJson { get; set; } 
        public decimal? MinRate { get; set; }            
        public decimal? MaxRate { get; set; }             
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
