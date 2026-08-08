using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Tutors.Commands.ApproveTutor
{
    public class ApproveTutorCommandHandler : IRequestHandler<ApproveTutorCommand, bool>
    {
        private readonly ITutorSearchRepository _tutorSearchRepository;
        private readonly IAdminRepository _adminRepository;

        public ApproveTutorCommandHandler(ITutorSearchRepository tutorSearchRepository, IAdminRepository adminRepository)
        {
            _tutorSearchRepository = tutorSearchRepository;
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(ApproveTutorCommand request, CancellationToken cancellationToken)
        {
            var success = await _tutorSearchRepository.ApproveTutorAsync(request.TutorUserId, request.AdminUserId);
            if (success)
            {
                await _adminRepository.SendTutorApprovalNotificationAsync(request.TutorUserId, isApproved: true);
            }
            return success;
        }
    }
}
