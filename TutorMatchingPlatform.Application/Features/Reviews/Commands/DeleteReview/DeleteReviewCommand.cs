using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommand : IRequest<bool>
    {
        public Guid ReviewId { get; set; }
        public Guid RequestorId { get; set; }
        public string RequestorRole { get; set; } = null!;
    }
}
