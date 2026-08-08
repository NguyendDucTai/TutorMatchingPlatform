using System;
using MediatR;
using TutorPlatform.Application.Contracts.Progress;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Application.Features.Progress.Queries.GetProgressChartData
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
