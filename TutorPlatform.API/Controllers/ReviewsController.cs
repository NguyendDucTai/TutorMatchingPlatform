using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Reviews;
using TutorPlatform.Application.Features.Reviews.Commands.CreateReview;
using TutorPlatform.Application.Features.Reviews.Commands.DeleteReview;
using TutorPlatform.Application.Features.Reviews.Queries.GetReviews;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Tutor,Student")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            command.ReviewerId = userId;
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<Guid>.Ok(response));
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviews(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetReviewsQuery
            {
                RevieweeId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(response));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(userRoleClaim))
            {
                return Unauthorized();
            }

            var command = new DeleteReviewCommand
            {
                ReviewId = id,
                RequestorId = userId,
                RequestorRole = userRoleClaim
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }
    }
}
