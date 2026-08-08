using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Guid>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;

        public CreateReviewCommandHandler(IReviewRepository reviewRepository, IBookingRepository bookingRepository)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new NotFoundException(nameof(Booking), request.BookingId);
            }

            // 1. Verify booking is completed
            if (booking.Status != BookingStatus.Completed)
            {
                throw new BadRequestException("Reviews can only be written for completed bookings.");
            }

            // 2. Verify 14-day deadline
            if (DateTime.UtcNow > booking.ScheduledEndAt.AddDays(14))
            {
                throw new BadRequestException("The review period for this booking has expired (maximum 14 days after completion).");
            }

            // 3. Verify participant role
            ReviewType reviewType;
            Guid revieweeId;

            if (request.ReviewerId == booking.StudentId)
            {
                reviewType = ReviewType.StudentToTutor;
                revieweeId = booking.TutorId;
            }
            else if (request.ReviewerId == booking.TutorId)
            {
                reviewType = ReviewType.TutorToStudent;
                revieweeId = booking.StudentId;
            }
            else
            {
                throw new ForbiddenException("You are not a participant of this booking.");
            }

            // 4. Verify no duplicate review
            var existingReview = await _reviewRepository.GetByBookingAndTypeAsync(request.BookingId, reviewType);
            if (existingReview != null)
            {
                throw new ConflictException("You have already submitted a review for this booking.");
            }

            // 5. Create Review
            var review = new Review(
                request.BookingId,
                request.ReviewerId,
                revieweeId,
                request.Rating,
                request.Comment,
                reviewType
            );

            await _reviewRepository.AddAsync(review);

            // 6. Recalculate Rating
            bool isTutor = (reviewType == ReviewType.StudentToTutor);
            await _reviewRepository.RecalculateAverageRatingAsync(revieweeId, isTutor);

            return review.Id;
        }
    }
}
