using MediatR;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommand : IRequest<UpdateMyProfileResult>
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        
        public FileUploadDto? AvatarFile { get; set; }

        // For Tutors
        public string? Bio { get; set; }

        // For Students
        public string? StudyGoals { get; set; }
        public string? TargetSubjectsJson { get; set; }
    }

    public class UpdateMyProfileResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
