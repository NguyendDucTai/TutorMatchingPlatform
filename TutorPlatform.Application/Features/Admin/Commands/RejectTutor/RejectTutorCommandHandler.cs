using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Commands.RejectTutor
{
    public class RejectTutorCommandHandler : IRequestHandler<RejectTutorCommand, bool>
    {
        private readonly IAdminRepository _adminRepository;
        private readonly INotificationSender _notificationSender;

        public RejectTutorCommandHandler(IAdminRepository adminRepository, INotificationSender notificationSender)
        {
            _adminRepository = adminRepository;
            _notificationSender = notificationSender;
        }

        public async Task<bool> Handle(RejectTutorCommand request, CancellationToken cancellationToken)
        {
            var success = await _adminRepository.RejectTutorAsync(request.TutorUserId);
            if (!success)
            {
                throw new NotFoundException("TutorProfile", request.TutorUserId);
            }

            // Save notification to DB
            await _adminRepository.SendTutorApprovalNotificationAsync(request.TutorUserId, isApproved: false, reason: request.Reason);

            // Build rejection message for SignalR push
            var rejectionMessage = "Hồ sơ của bạn không được phê duyệt.";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                rejectionMessage += $" Lý do từ chối: {request.Reason.Trim()}";
            }
            else
            {
                rejectionMessage += " Vui lòng cập nhật thông tin giới thiệu bản thân hoặc bằng cấp và gửi duyệt lại.";
            }

            // Push real-time via SignalR so tutor's bell updates dynamically
            var dto = new NotificationDto
            {
                Id = Guid.NewGuid(),
                UserId = request.TutorUserId,
                Title = "Hồ sơ của bạn không được phê duyệt",
                Message = rejectionMessage,
                Type = "TutorRejected",
                RelatedEntityType = "TutorApproval",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationSender.SendNotificationAsync(request.TutorUserId, dto);

            return true;
        }
    }
}
