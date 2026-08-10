using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Application.Features.Bookings.Commands.UpdateMeetingLink
{
    public class UpdateMeetingLinkCommandHandler : IRequestHandler<UpdateMeetingLinkCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public UpdateMeetingLinkCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<bool> Handle(UpdateMeetingLinkCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.TutorId) throw new ForbiddenException("You can only update your own bookings.");
            
            // Allow update meeting link if status is Confirmed
            if (booking.Status != BookingStatus.Confirmed) 
                throw new BadRequestException("Meeting link can only be updated for confirmed bookings.");

            // Directly access and update the property (since MeetingLink is private set, we need a method in entity)
            booking.UpdateMeetingLink(request.MeetingLink);

            await _bookingRepository.UpdateAsync(booking);

            // Fetch tutor name and publish meeting link updated event
            var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
            var tutorName = tutor?.FullName ?? "Gia sư";

            await _publisher.Publish(new TutorPlatform.Application.Features.Notifications.Events.MeetingLinkUpdatedEvent
            {
                Booking = booking,
                TutorName = tutorName,
                NewMeetingLink = request.MeetingLink
            }, cancellationToken);

            return true;
        }
    }
}
