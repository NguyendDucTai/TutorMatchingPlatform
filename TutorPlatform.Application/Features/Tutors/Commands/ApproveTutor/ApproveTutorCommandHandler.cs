using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Tutors.Commands.ApproveTutor
{
    public class ApproveTutorCommandHandler : IRequestHandler<ApproveTutorCommand, bool>
    {
        private readonly ITutorSearchRepository _tutorSearchRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly INotificationSender _notificationSender;

        public ApproveTutorCommandHandler(ITutorSearchRepository tutorSearchRepository, IAdminRepository adminRepository, INotificationSender notificationSender)
        {
            _tutorSearchRepository = tutorSearchRepository;
            _adminRepository = adminRepository;
            _notificationSender = notificationSender;
        }

        public async Task<bool> Handle(ApproveTutorCommand request, CancellationToken cancellationToken)
        {
            var success = await _tutorSearchRepository.ApproveTutorAsync(request.TutorUserId, request.AdminUserId);
            if (success)
            {
                // Save notification to DB
                await _adminRepository.SendTutorApprovalNotificationAsync(request.TutorUserId, isApproved: true);

                // Push real-time via SignalR so tutor's bell updates dynamically
                var dto = new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = request.TutorUserId,
                    Title = "Hồ sơ của bạn đã được duyệt!",
                    Message = "Chúc mừng bạn! Hồ sơ gia sư của bạn đã được phê duyệt thành công. Bây giờ bạn có thể bắt đầu nhận học viên.",
                    Type = "TutorApproved",
                    RelatedEntityType = "TutorApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationSender.SendNotificationAsync(request.TutorUserId, dto);
            }
            return success;
        }
    }
}
