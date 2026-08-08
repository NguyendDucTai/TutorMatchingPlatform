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

        public UpdateMeetingLinkCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
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

            return true;
        }
    }
}
