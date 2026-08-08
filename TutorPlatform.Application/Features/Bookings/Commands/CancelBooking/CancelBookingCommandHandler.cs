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

        public CancelBookingCommandHandler(IBookingRepository bookingRepository, ICreditService creditService, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _creditService = creditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.UserId && booking.StudentId != request.UserId)
            {
                throw new ForbiddenException("You are not part of this booking.");
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

            return true;
        }
    }
}
