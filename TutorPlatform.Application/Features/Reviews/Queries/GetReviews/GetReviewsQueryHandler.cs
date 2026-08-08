using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Reviews;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Reviews.Queries.GetReviews
{
    public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, PagedResult<ReviewDto>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewsQueryHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PagedResult<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            var pagedResult = await _reviewRepository.GetReviewsWithDetailsAsync(
                request.RevieweeId,
                request.PageNumber,
                request.PageSize
            );

            var dtos = pagedResult.Items.Select(x => new ReviewDto
            {
                Id = x.Review.Id,
                BookingId = x.Review.BookingId,
                ReviewerId = x.Review.ReviewerId,
                ReviewerName = x.ReviewerName,
                ReviewerAvatarUrl = x.ReviewerAvatar,
                RevieweeId = x.Review.RevieweeId,
                Rating = x.Review.Rating,
                Comment = x.Review.Comment,
                ReviewType = x.Review.ReviewType.ToString(),
                SubjectName = x.SubjectName,
                CreatedAt = x.Review.CreatedAt
            }).ToList();

            return new PagedResult<ReviewDto>(dtos, pagedResult.TotalCount);
        }
    }
}
