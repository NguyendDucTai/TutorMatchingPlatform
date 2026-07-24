using MediatR;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommand : IRequest<UpdateMyProfileResult>
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        
        public FileUploadDto? AvatarFile { get; set; }

    }

    public class UpdateMyProfileResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
