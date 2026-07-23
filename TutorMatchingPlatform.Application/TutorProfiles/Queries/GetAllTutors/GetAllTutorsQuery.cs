using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.TutorProfiles.DTOs;

namespace TutorMatchingPlatform.Application.TutorProfiles.Queries.GetAllTutors
{
    public class GetAllTutorsQuery : IRequest<GetAllTutorsResult>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllTutorsResult
    {
        public List<TutorProfileDto> Tutors { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
