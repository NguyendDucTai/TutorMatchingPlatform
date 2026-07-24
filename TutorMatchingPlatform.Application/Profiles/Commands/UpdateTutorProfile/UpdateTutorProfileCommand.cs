using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? Bio { get; set; }
        public string? QualificationsText { get; set; }
        public List<FileUploadDto> QualificationFiles { get; set; } = new List<FileUploadDto>();
    }
}
