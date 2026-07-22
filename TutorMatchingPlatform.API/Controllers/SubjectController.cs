using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Subjects.Commands.CreateSubject;
using TutorMatchingPlatform.Application.Subjects.Commands.UpdateSubject;
using TutorMatchingPlatform.Application.Subjects.Commands.DeleteSubject;
using TutorMatchingPlatform.Application.Subjects.Queries.GetAllSubjects;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    public class SubjectController : ControllerBase
    {
        private readonly ISender _sender;

        public SubjectController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
        {
            var result = await _sender.Send(new GetAllSubjectsQuery { OnlyActive = onlyActive });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequestDto request)
        {
            var command = new UpdateSubjectCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await _sender.Send(command);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(new { Success = result });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var command = new DeleteSubjectCommand { Id = id };
            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public class CreateSubjectRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateSubjectRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
