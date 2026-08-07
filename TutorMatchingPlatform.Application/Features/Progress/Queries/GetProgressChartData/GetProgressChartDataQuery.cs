using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Progress;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Features.Progress.Queries.GetProgressChartData
{
    public class GetProgressChartDataQuery : IRequest<ProgressChartDto>
    {
        public Guid? StudentId { get; set; }
        public Guid SubjectId { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid CurrentUserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public UserRole Role { get; set; }
    }
}
