    using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors;
using TutorMatchingPlatform.Application.Tutors.Queries.GetTutorDetails;
using TutorMatchingPlatform.Application.Tutors.Queries.GetTutorFeedbacks;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/tutor")]
    public class TutorController : ControllerBase
    {
        private readonly ISender _sender;

        public TutorController(ISender sender)
        {
            _sender = sender;
        }



        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] int subjectId,
            [FromQuery] decimal? minRate,
            [FromQuery] decimal? maxRate,
            [FromQuery] string? studentScheduleJson,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new SearchTutorsQuery
            {
                SubjectId = subjectId,
                MinRate = minRate,
                MaxRate = maxRate,
                StudentScheduleJson = studentScheduleJson,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _sender.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTutorDetails(int id)
        {
            var query = new GetTutorDetailsQuery { TutorId = id };
            var result = await _sender.Send(query);
            
            if (result == null)
            {
                return NotFound(new { Message = "Tutor not found." });
            }
            
            return Ok(result);
        }

        [HttpGet("{id}/feedbacks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTutorFeedbacks(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetTutorFeedbacksQuery 
            { 
                TutorId = id, 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };
            var result = await _sender.Send(query);
            
            if (result == null)
            {
                return NotFound(new { Message = "Tutor not found." });
            }
            
            return Ok(result);
        }

        [HttpPut("me/availability")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> UpdateAvailability([FromBody] TutorMatchingPlatform.Application.Tutors.Commands.UpdateTutorAvailability.UpdateTutorAvailabilityCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            command.UserId = userId;
            var result = await _sender.Send(command);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update availability. Make sure your tutor profile is set up." });
            }

            return Ok(new { Success = true });
        }
    }
}
