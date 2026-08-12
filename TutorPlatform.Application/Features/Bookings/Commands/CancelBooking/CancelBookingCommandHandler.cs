using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICreditService _creditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            ICreditService creditService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _creditService = creditService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.UserId && booking.StudentId != request.UserId)
            {
                throw new ForbiddenException("You are not part of this booking.");
            }

            // Anti-fraud check: Prevent student from cancelling if the scheduled start time has arrived or passed
            if (request.UserId == booking.StudentId)
            {
                var nowVietnam = System.DateTime.UtcNow.AddHours(7);
                if (nowVietnam >= booking.ScheduledStartAt || booking.Status == Domain.Enums.BookingStatus.Completed || booking.Status == Domain.Enums.BookingStatus.Cancelled)
                {
                    throw new BadRequestException("Buổi học đã diễn ra hoặc đã bắt đầu, học viên không thể hủy buổi học.");
                }
            }

            booking.Cancel(request.UserId, request.Reason);

            // Bug #6: Wrap refund + update in a single transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _creditService.RefundAsync(booking.StudentId, booking.CreditAmount, "Refund for cancelled booking", booking.Id);
                await _bookingRepository.UpdateAsync(booking);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // Fetch canceller user name and publish notification event
            var cancellerUser = await _userRepository.GetByIdAsync(request.UserId);
            var cancellerName = cancellerUser?.FullName ?? (request.UserId == booking.TutorId ? "Gia sư" : "Học viên");

            await _publisher.Publish(new TutorPlatform.Application.Features.Notifications.Events.BookingCancelledEvent
            {
                Booking = booking,
                CancelledByUserId = request.UserId,
                CancelledByUserName = cancellerName
            }, cancellationToken);

            return true;
        }
    }
}
