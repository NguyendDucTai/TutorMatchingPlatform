using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Sessions.Commands.BookSession;
using TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange;
using TutorMatchingPlatform.Application.Sessions.Commands.RespondToSessionChange;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/session")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISender _sender;

        public SessionController(ISender sender)
        {
            _sender = sender;
        }

        // UC-04: Book a Session (Student only)
        [HttpPost("book")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> BookSession([FromBody] BookSessionRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int studentId))
            {
                return Unauthorized();
            }

            var command = new BookSessionCommand
            {
                StudentId = studentId,
                TutorId = request.TutorId,
                SubjectId = request.SubjectId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // UC-05: Propose Reschedule / Cancel (Student or Tutor)
        [HttpPost("{id:int}/propose-change")]
        public async Task<IActionResult> ProposeChange(int id, [FromBody] ProposeChangeRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int requesterId))
            {
                return Unauthorized();
            }

            var command = new ProposeSessionChangeCommand
            {
                SessionId = id,
                RequesterUserId = requesterId,
                ChangeType = request.ChangeType,
                NewStartTime = request.NewStartTime,
                NewEndTime = request.NewEndTime
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }

        // UC-06: Accept / Decline a change proposal (Student or Tutor)
        [HttpPost("change-requests/{changeRequestId:int}/respond")]
        public async Task<IActionResult> RespondToChange(int changeRequestId, [FromBody] RespondChangeRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int responderId))
            {
                return Unauthorized();
            }

            var command = new RespondToSessionChangeCommand
            {
                ChangeRequestId = changeRequestId,
                ResponderUserId = responderId,
                Accept = request.Accept
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }
    }

    // DTOs
    public class BookSessionRequestDto
    {
        public int TutorId { get; set; }
        public int SubjectId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ProposeChangeRequestDto
    {
        public SessionChangeType ChangeType { get; set; }
        public DateTime? NewStartTime { get; set; }
        public DateTime? NewEndTime { get; set; }
    }

    public class RespondChangeRequestDto
    {
        public bool Accept { get; set; }
    }
}
