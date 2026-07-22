using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Subjects.Commands.CreateSubject;
using TutorMatchingPlatform.Application.Subjects.Commands.UpdateSubject;
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectCommand command)
        {
            command.Id = id;
            var result = await _sender.Send(command);
            return Ok(new { Success = result });
        }
    }
}
