using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
            }

            user.UpdateProfile(request.FullName, request.Phone, request.AvatarUrl ?? user.AvatarUrl);

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
