using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Admin.Commands.RejectTutor
{
    public class RejectTutorCommandHandler : IRequestHandler<RejectTutorCommand, bool>
    {
        private readonly IAdminRepository _adminRepository;

        public RejectTutorCommandHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(RejectTutorCommand request, CancellationToken cancellationToken)
        {
            var success = await _adminRepository.RejectTutorAsync(request.TutorUserId);
            if (!success)
            {
                throw new NotFoundException("TutorProfile", request.TutorUserId);
            }

            // Send notification to the tutor
            await _adminRepository.SendTutorApprovalNotificationAsync(request.TutorUserId, isApproved: false);

            return true;
        }
    }
}
