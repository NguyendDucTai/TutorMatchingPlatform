using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Progress;
using TutorPlatform.Application.Features.Progress.Commands.CreateLearningGoal;
using TutorPlatform.Application.Features.Progress.Commands.CreateSessionRecord;
using TutorPlatform.Application.Features.Progress.Commands.RecordGoalProgress;
using TutorPlatform.Application.Features.Progress.Commands.UpdateLearningGoal;
using TutorPlatform.Application.Features.Progress.Queries.GetLearningGoals;
using TutorPlatform.Application.Features.Progress.Queries.GetProgressChartData;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("goals")]
        [Authorize(Roles = "Tutor")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateLearningGoal([FromBody] CreateLearningGoalCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.TutorId = tutorId;
            var response = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<Guid>.Created(response));
        }

        [HttpPut("goals/{id}")]
        [Authorize(Roles = "Tutor")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLearningGoal(Guid id, [FromBody] UpdateLearningGoalCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.GoalId = id;
            command.TutorId = tutorId;
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPost("goals/{id}/record")]
        [Authorize(Roles = "Tutor")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecordGoalProgress(Guid id, [FromBody] RecordGoalProgressCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.GoalId = id;
            command.TutorId = tutorId;
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPost("sessions/{bookingId}")]
        [Authorize(Roles = "Tutor")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSessionRecord(Guid bookingId, [FromBody] CreateSessionRecordCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.BookingId = bookingId;
            command.TutorId = tutorId;
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<Guid>.Ok(response));
        }

        [HttpGet("goals")]
        [ProducesResponseType(typeof(ApiResponse<List<LearningGoalDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLearningGoals([FromQuery] Guid? studentId, [FromQuery] Guid? subjectId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized();
            }

            var query = new GetLearningGoalsQuery
            {
                StudentId = studentId,
                SubjectId = subjectId,
                CurrentUserId = userId,
                Role = Enum.Parse<UserRole>(roleClaim)
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<List<LearningGoalDto>>.Ok(response));
        }

        [HttpGet("chart-data")]
        [ProducesResponseType(typeof(ApiResponse<ProgressChartDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProgressChartData([FromQuery] Guid? studentId, [FromQuery] Guid subjectId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized();
            }

            var query = new GetProgressChartDataQuery
            {
                StudentId = studentId,
                SubjectId = subjectId,
                CurrentUserId = userId,
                Role = Enum.Parse<UserRole>(roleClaim)
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<ProgressChartDto>.Ok(response));
        }
    }
}
