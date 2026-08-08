using System;
using MediatR;
using TutorPlatform.Application.Contracts.Tutors;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Application.Features.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQuery : IRequest<PagedResult<TutorSearchResultDto>>
    {
        public Guid? SubjectId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public DateTime? SpecificDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? SearchQuery { get; set; }
        public string? SearchTerm { get => SearchQuery; set => SearchQuery = value; }
        public string? SortBy { get; set; }
        public decimal? MinRating { get; set; }
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
