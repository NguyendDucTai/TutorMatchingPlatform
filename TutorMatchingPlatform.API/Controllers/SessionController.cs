using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Sessions.Commands.BookSession;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/session")]
    [Authorize(Roles = "Student")]
    public class SessionController : ControllerBase
    {
        private readonly ISender _sender;

        public SessionController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookSession([FromBody] BookSessionCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int studentId))
            {
                return Unauthorized();
            }

            command.StudentId = studentId;

            var result = await _sender.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
