using System;
using MediatR;

namespace TutorPlatform.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommand : IRequest<Guid>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid ReviewerId { get; set; }

        public Guid BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
