using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Auth.Commands.Register;
using TutorMatchingPlatform.Application.Auth.Queries.Login;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
    }
}
