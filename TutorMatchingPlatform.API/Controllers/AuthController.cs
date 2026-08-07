using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.API.Common;
using TutorMatchingPlatform.API.Models.Auth;
using TutorMatchingPlatform.Application.Contracts.Auth;
using TutorMatchingPlatform.Application.Features.Auth.Commands.ChangePassword;
using TutorMatchingPlatform.Application.Features.Auth.Commands.Login;
using TutorMatchingPlatform.Application.Features.Auth.Commands.RefreshToken;
using TutorMatchingPlatform.Application.Features.Auth.Commands.Register;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            if (response == null)
            {
                return BadRequest(ApiResponse<AuthResponse>.Error(400, "Email hoặc mật khẩu không chính xác."));
            }
            return Ok(ApiResponse<AuthResponse>.Ok(response));
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            [FromServices] IUserRepository userRepository)
        {
            var existingUser = await userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<AuthResponse>.Error(400, "Email already exists."));
            }

            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName,
                Role = request.Role
            };

            var response = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponse>.Created(response));
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var command = new RefreshTokenCommand
            {
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<AuthResponse>.Ok(response));
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }
    }
}
