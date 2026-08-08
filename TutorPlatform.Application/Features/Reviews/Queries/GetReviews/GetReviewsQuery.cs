using System;
using MediatR;
using TutorPlatform.Domain.Common;
using TutorPlatform.Application.Contracts.Reviews;

namespace TutorPlatform.Application.Features.Reviews.Queries.GetReviews
{
    public class GetReviewsQuery : IRequest<PagedResult<ReviewDto>>
    {
        public Guid RevieweeId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
