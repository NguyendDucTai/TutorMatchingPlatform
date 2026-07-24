using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Both Tutor and Student can rate
    public class FeedbackController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeedbackController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("rate")]
        public async Task<IActionResult> RateSession([FromBody] RateSessionRequestDto request)
        {
            // Extract SenderUserId from JWT Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int senderId))
            {
                return Unauthorized();
            }

            var command = new RateSessionCommand
            {
                SessionId = request.SessionId,
                SenderUserId = senderId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }
    }

    public class RateSessionRequestDto
    {
        public int SessionId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
