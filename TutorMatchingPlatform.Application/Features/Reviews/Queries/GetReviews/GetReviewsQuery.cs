using System;
using MediatR;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Application.Contracts.Reviews;

namespace TutorMatchingPlatform.Application.Features.Reviews.Queries.GetReviews
{
    public class GetReviewsQuery : IRequest<PagedResult<ReviewDto>>
    {
        public Guid RevieweeId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
