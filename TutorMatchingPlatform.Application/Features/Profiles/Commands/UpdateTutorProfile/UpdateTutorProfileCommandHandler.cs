using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommandHandler : IRequestHandler<UpdateTutorProfileCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateTutorProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdateTutorProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _userRepository.GetTutorProfileAsync(request.UserId);
            
            if (profile == null)
            {
                profile = new Domain.Entities.TutorProfile(request.UserId, null, null);
                profile.Approve(request.UserId);
                await _userRepository.AddTutorProfileAsync(profile);
            }

            profile.UpdateDetails(request.Bio, request.Qualifications);
            if (request.DefaultMeetingLink != null) 
            {
                profile.UpdateDefaultMeetingLink(request.DefaultMeetingLink);
            }

            await _userRepository.UpdateTutorProfileAsync(profile);

            return true;
        }
    }
}
