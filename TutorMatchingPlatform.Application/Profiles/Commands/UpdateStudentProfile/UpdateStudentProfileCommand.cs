using MediatR;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateStudentProfile
{
    public class UpdateStudentProfileCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? StudyGoals { get; set; }
        public string? TargetSubjectsJson { get; set; }
    }
}
