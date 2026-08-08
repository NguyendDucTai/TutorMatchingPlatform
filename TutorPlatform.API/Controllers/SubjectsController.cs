using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Subjects;
using TutorPlatform.Application.Features.Subjects.Commands.CreateSubject;
using TutorPlatform.Application.Features.Subjects.Queries.GetAllSubjects;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SubjectDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSubjects([FromQuery] bool includeInactive = false)
        {
            var query = new GetSubjectsQuery { IncludeInactive = includeInactive };
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<List<SubjectDto>>.Ok(response));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<SubjectDto>.Created(response));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] TutorPlatform.Application.Features.Subjects.Commands.UpdateSubject.UpdateSubjectCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Subject ID mismatch."));
            }
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<SubjectDto>.Ok(response));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var command = new TutorPlatform.Application.Features.Subjects.Commands.DeleteSubject.DeleteSubjectCommand(id);
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReactivateSubject(Guid id)
        {
            var command = new TutorPlatform.Application.Features.Subjects.Commands.ReactivateSubject.ReactivateSubjectCommand(id);
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<SubjectDto>.Ok(response));
        }
    }
}
