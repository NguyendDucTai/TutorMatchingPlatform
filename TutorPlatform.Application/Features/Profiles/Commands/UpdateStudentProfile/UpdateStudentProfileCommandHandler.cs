using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateStudentProfile
{
    public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateStudentProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _userRepository.GetStudentProfileAsync(request.UserId);
            
            if (profile == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.StudentProfile), request.UserId);
            }

            profile.UpdateDetails(request.GradeLevel ?? profile.GradeLevel, request.LearningPreferences ?? profile.LearningPreferences); // fallback to existing

            await _userRepository.UpdateStudentProfileAsync(profile);

            return true;
        }
    }
}
