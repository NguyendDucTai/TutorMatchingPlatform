using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Features.Bookings.Commands.CancelBooking;
using TutorPlatform.Application.Features.Bookings.Commands.CompleteBooking;
using TutorPlatform.Application.Features.Bookings.Commands.ConfirmBooking;
using TutorPlatform.Application.Features.Bookings.Commands.CreateBooking;
using TutorPlatform.Application.Features.Bookings.Queries.GetMyBookings;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<Guid>> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var studentId))
            {
                return Unauthorized();
            }

            command.StudentId = studentId;
            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                if (result.IsConflict)
                {
                    return Conflict(new { messages = new[] { result.ErrorMessage } });
                }
                return BadRequest(new { messages = new[] { result.ErrorMessage } });
            }

            return Ok(result.BookingId);
        }

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Tutor")]
        public async Task<ActionResult> ConfirmBooking(Guid id, [FromBody] ConfirmBookingCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.BookingId = id;
            command.TutorId = tutorId;
            try
            {
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ConflictException ex)
            {
                return Conflict(new { messages = new[] { ex.Message } });
            }
        }

        [HttpPut("{id}/meeting-link")]
        [Authorize(Roles = "Tutor")]
        public async Task<ActionResult> UpdateMeetingLink(Guid id, [FromBody] TutorPlatform.Application.Features.Bookings.Commands.UpdateMeetingLink.UpdateMeetingLinkCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.BookingId = id;
            command.TutorId = tutorId;
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelBooking(Guid id, [FromBody] CancelBookingCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            command.BookingId = id;
            command.UserId = userId;
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Tutor")]
        public async Task<ActionResult> CompleteBooking(Guid id, [FromBody] CompleteBookingCommand command)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            command.BookingId = id;
            command.TutorId = tutorId;
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult> GetMyBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized();
            }

            var query = new GetMyBookingsQuery
            {
                UserId = userId,
                Role = Enum.Parse<UserRole>(roleClaim),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
