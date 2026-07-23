using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Auth.Commands.Register;
using TutorMatchingPlatform.Application.Auth.Commands.ForgotPassword;
using TutorMatchingPlatform.Application.Auth.Commands.ResetPassword;
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

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TutorMatchingPlatform.Application.Auth.Queries.RefreshToken.RefreshTokenQuery query)
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Step 1: Request a password reset token. Token is emailed (or returned in ResetToken field during dev).
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Step 2: Use the token received from forgot-password to set a new password.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _sender.Send(command);
            if (!result.Success)
            {
                return BadRequest(new { result.Message });
            }
            return Ok(new { result.Message });
        }
    }
}
