using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Domain.Interfaces;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Features.Bookings.Commands.ConfirmBooking
{
    public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public ConfirmBookingCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<bool> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.TutorId) throw new ForbiddenException("You can only confirm your own bookings.");

            booking.Confirm(request.MeetingLink);

            await _bookingRepository.UpdateAsync(booking);

            // Fetch tutor name and publish confirmation notification
            var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
            var tutorName = tutor?.FullName ?? "Gia sư";

            await _publisher.Publish(new TutorMatchingPlatform.Application.Features.Notifications.Events.BookingConfirmedEvent
            {
                Booking = booking,
                TutorName = tutorName
            }, cancellationToken);

            return true;
        }
    }
}
