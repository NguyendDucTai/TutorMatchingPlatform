using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Bookings.Commands.SubmitComplaint
{
    public class SubmitComplaintCommandHandler : IRequestHandler<SubmitComplaintCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public SubmitComplaintCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> Handle(SubmitComplaintCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new NotFoundException(nameof(Booking), request.BookingId);
            }

            if (booking.StudentId != request.StudentId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi khiếu nại cho buổi học này.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                throw new BadRequestException("Chỉ có thể gửi khiếu nại cho buổi học đã hoàn thành.");
            }

            if (!string.IsNullOrEmpty(booking.CancellationReason) && booking.CancellationReason.Contains("[KHIẾU NẠI]"))
            {
                throw new BadRequestException("Bạn đã gửi khiếu nại cho buổi học này rồi. Mỗi buổi học chỉ được phép khiếu nại 1 lần duy nhất.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new BadRequestException("Vui lòng nhập lý do khiếu nại.");
            }

            if (request.Reason.Trim().Length > 1000)
            {
                throw new BadRequestException("Nội dung khiếu nại không được vượt quá 1000 ký tự.");
            }

            var cleanReason = request.Reason.Trim();
            var student = await _userRepository.GetByIdAsync(booking.StudentId);
            var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
            var studentName = student?.FullName ?? "Học viên";
            var tutorName = tutor?.FullName ?? "Gia sư";
            var bookingCode = booking.Id.ToString().Substring(0, Math.Min(8, booking.Id.ToString().Length));

            // Record complaint tag on booking cancellationReason (max 500 chars for DB)
            var complaintReasonText = $"[KHIẾU NẠI]: {cleanReason}";
            if (complaintReasonText.Length > 500)
            {
                complaintReasonText = complaintReasonText.Substring(0, 497) + "...";
            }
            
            var reasonProperty = typeof(Booking).GetProperty("CancellationReason");
            if (reasonProperty != null && reasonProperty.CanWrite)
            {
                reasonProperty.SetValue(booking, complaintReasonText);
            }

            await _bookingRepository.UpdateAsync(booking);

            // Format notification messages safely (max 1000 chars for DB column)
            var adminMessage = $"Học viên {studentName} đã gửi khiếu nại đối với Gia sư {tutorName} (Buổi học #{bookingCode}). Lý do: \"{cleanReason}\". Vui lòng xem xét và xử lý cảnh cáo gia sư.";
            if (adminMessage.Length > 1000)
            {
                adminMessage = adminMessage.Substring(0, 995) + "...";
            }

            var tutorMessage = $"Bạn vừa nhận được 1 khiếu nại từ học viên {studentName} cho buổi học #{bookingCode}. Lý do: \"{cleanReason}\". Ban quản trị (Admin) đang xem xét trường hợp này.";
            if (tutorMessage.Length > 1000)
            {
                tutorMessage = tutorMessage.Substring(0, 995) + "...";
            }

            // 1. Notify all Admins
            var admins = await _userRepository.GetUsersByRoleAsync(UserRole.Admin);
            foreach (var admin in admins)
            {
                var adminNotification = new Notification(
                    admin.Id,
                    $"🚩 Khiếu nại từ học viên {studentName}",
                    adminMessage,
                    NotificationType.System,
                    booking.Id,
                    "Booking"
                );
                await _notificationRepository.AddAsync(adminNotification);
            }

            // 2. Send Warning Notification to Tutor
            var tutorWarningNotification = new Notification(
                booking.TutorId,
                "⚠️ Cảnh cáo khiếu nại từ học viên",
                tutorMessage,
                NotificationType.System,
                booking.Id,
                "Booking"
            );
            await _notificationRepository.AddAsync(tutorWarningNotification);

            return true;
        }
    }
}
