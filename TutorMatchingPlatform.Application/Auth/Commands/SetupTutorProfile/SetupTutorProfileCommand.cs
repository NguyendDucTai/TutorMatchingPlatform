using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Auth.Commands.SetupTutorProfile
{
    public class SetupTutorProfileCommand : IRequest<bool>
    {
        public int UserId { get; set; } // Set by Controller from Claims
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? QualificationsText { get; set; }
        public List<SubjectRateDto> Subjects { get; set; } = new List<SubjectRateDto>();
        public string? FreeSchedulesJson { get; set; }
        
        // Files
        public FileUploadDto? AvatarFile { get; set; }
        public List<FileUploadDto> QualificationFiles { get; set; } = new List<FileUploadDto>();
    }
}
