using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Commands.RejectTutor
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
