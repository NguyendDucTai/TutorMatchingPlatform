using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.Application.Contracts.Availabilities;
using TutorPlatform.Application.Features.Availabilities.Commands.UpdateAvailability;
using TutorPlatform.Application.Features.Availabilities.Queries.GetAvailabilities;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AvailabilitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{tutorId}")]
        public async Task<ActionResult<List<AvailabilityDto>>> GetAvailabilities(Guid tutorId)
        {
            var query = new GetAvailabilitiesQuery { TutorId = tutorId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [Authorize(Roles = "Tutor")]
        [HttpPut("my-availability")]
        public async Task<ActionResult> UpdateMyAvailability([FromBody] List<UpdateAvailabilityItemDto> availabilities)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            var command = new UpdateAvailabilityCommand
            {
                TutorId = tutorId,
                Availabilities = availabilities
            };

            await _mediator.Send(command);
            return NoContent();
        }
    }
}
