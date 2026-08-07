using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Progress.Commands.CreateSessionRecord
{
    public class CreateSessionRecordCommandHandler : IRequestHandler<CreateSessionRecordCommand, Guid>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISessionRecordRepository _sessionRecordRepository;

        public CreateSessionRecordCommandHandler(
            IBookingRepository bookingRepository,
            ISessionRecordRepository sessionRecordRepository)
        {
            _bookingRepository = bookingRepository;
            _sessionRecordRepository = sessionRecordRepository;
        }

        public async Task<Guid> Handle(CreateSessionRecordCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new NotFoundException(nameof(Booking), request.BookingId);
            }

            // Verify owner
            if (booking.TutorId != request.TutorId)
            {
                throw new ForbiddenException("You are not authorized to create a session record for this booking.");
            }

            // Verify completed status
            if (booking.Status != BookingStatus.Completed)
            {
                throw new BadRequestException("Session records can only be created for completed bookings.");
            }

            // Verify start time (prevent writing session records before the session has started)
            var scheduledStart = booking.ScheduledStartAt;
            bool isFutureUtc = DateTime.UtcNow < DateTime.SpecifyKind(scheduledStart, DateTimeKind.Utc);
            bool isFutureLocal = DateTime.Now < DateTime.SpecifyKind(scheduledStart, DateTimeKind.Local);
            if (isFutureUtc && isFutureLocal)
            {
                throw new BadRequestException("Cannot write session feedback before the scheduled start time.");
            }

            var existingRecord = await _sessionRecordRepository.GetByBookingIdAsync(request.BookingId);
            if (existingRecord != null)
            {
                existingRecord.UpdateDetails(
                    request.Score,
                    request.CompletionPercentage,
                    request.TutorNotes,
                    request.Strengths,
                    request.AreasForImprovement
                );
                await _sessionRecordRepository.UpdateAsync(existingRecord);
                return existingRecord.Id;
            }
            else
            {
                var record = new SessionRecord(
                    request.BookingId,
                    request.Score,
                    request.CompletionPercentage,
                    request.TutorNotes,
                    request.Strengths,
                    request.AreasForImprovement
                );
                await _sessionRecordRepository.AddAsync(record);
                return record.Id;
            }
        }
    }
}
