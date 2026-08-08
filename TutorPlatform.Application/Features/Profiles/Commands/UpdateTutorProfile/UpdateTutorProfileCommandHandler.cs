using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommandHandler : IRequestHandler<UpdateTutorProfileCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;

        public UpdateTutorProfileCommandHandler(IUserRepository userRepository, IAdminRepository adminRepository)
        {
            _userRepository = userRepository;
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(UpdateTutorProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _userRepository.GetTutorProfileAsync(request.UserId);
            
            if (profile == null)
            {
                profile = new Domain.Entities.TutorProfile(request.UserId, null, null);
                await _userRepository.AddTutorProfileAsync(profile);
            }

            profile.UpdateDetails(request.Bio, request.Qualifications);
            if (request.DefaultMeetingLink != null) 
            {
                profile.UpdateDefaultMeetingLink(request.DefaultMeetingLink);
            }

            await _userRepository.UpdateTutorProfileAsync(profile);

            // Check if profile is complete (profile details + availability) and notify admin
            await _adminRepository.CheckAndNotifyAdminOnTutorProfileCompletionAsync(request.UserId);

            return true;
        }
    }
}
