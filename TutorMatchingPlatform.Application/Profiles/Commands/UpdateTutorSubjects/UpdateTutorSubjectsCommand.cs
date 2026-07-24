using System.Collections.Generic;
using MediatR;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorSubjects
{
    public class SubjectRateDto
    {
        public int SubjectId { get; set; }
        public decimal Rate { get; set; }
    }

    public class UpdateTutorSubjectsCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public List<SubjectRateDto> Subjects { get; set; } = new List<SubjectRateDto>();
    }
}
