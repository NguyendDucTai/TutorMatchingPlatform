using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Progress.Commands.SetLearningGoal;
using TutorMatchingPlatform.Application.Progress.Commands.RecordSessionResult;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/progress")]
    [Authorize(Roles = "Tutor")]
    public class ProgressController : ControllerBase
    {
        private readonly ISender _sender;

        public ProgressController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("goal")]
        public async Task<IActionResult> SetGoal([FromBody] SetLearningGoalCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int tutorId))
            {
                return Unauthorized();
            }

            command.TutorId = tutorId;

            var goalId = await _sender.Send(command);
            return Ok(new { Success = true, GoalId = goalId });
        }

        /// <summary>
        /// Record session score, feedback and goal completion % after session is Completed (UC-09)
        /// </summary>
        [HttpPost("record-result")]
        public async Task<IActionResult> RecordResult([FromBody] RecordSessionResultCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int tutorUserId))
            {
                return Unauthorized();
            }

            command.TutorUserId = tutorUserId;

            var result = await _sender.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
