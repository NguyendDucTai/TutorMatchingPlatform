using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
    {
        private readonly IReviewRepository _reviewRepository;

        public DeleteReviewCommandHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify role is Admin
            if (request.RequestorRole != "Admin")
            {
                throw new ForbiddenException("Only administrators can delete reviews.");
            }

            // 2. Find review
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                throw new NotFoundException(nameof(Review), request.ReviewId);
            }

            // 3. Delete Review
            await _reviewRepository.DeleteAsync(review);

            // 4. Recalculate average rating of the reviewee
            bool isTutor = (review.ReviewType == ReviewType.StudentToTutor);
            await _reviewRepository.RecalculateAverageRatingAsync(review.RevieweeId, isTutor);

            return true;
        }
    }
}
