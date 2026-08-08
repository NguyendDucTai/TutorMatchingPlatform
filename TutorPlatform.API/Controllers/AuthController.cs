using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.API.Models.Auth;
using TutorPlatform.Application.Contracts.Auth;
using TutorPlatform.Application.Features.Auth.Commands.ChangePassword;
using TutorPlatform.Application.Features.Auth.Commands.ForgotPassword;
using TutorPlatform.Application.Features.Auth.Commands.Login;
using TutorPlatform.Application.Features.Auth.Commands.RefreshToken;
using TutorPlatform.Application.Features.Auth.Commands.Register;
using TutorPlatform.Application.Features.Auth.Commands.ResetPasswordWithOtp;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.API.Controllers
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
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] TutorPlatform.Application.Common.Interfaces.IPasswordHasher passwordHasher)
        {
            var user = await userRepository.GetByEmailAsync(request.Email);
            if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return BadRequest(ApiResponse<AuthResponse>.Error(400, "Email hoặc mật khẩu không chính xác."));
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<AuthResponse>.Error(403, "Tài khoản của bạn đã bị vô hiệu hóa hoặc sa thải."));
            }

            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<AuthResponse>.Ok(response));
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName,
                Role = request.Role
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<AuthResponse>.Ok(response));
        }

        [HttpPost("refresh-token")]
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

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var command = new ForgotPasswordCommand
            {
                Email = request.Email
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [AllowAnonymous]
        [HttpPost("reset-password-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request)
        {
            var command = new ResetPasswordWithOtpCommand
            {
                Email = request.Email,
                OtpCode = request.OtpCode,
                NewPassword = request.NewPassword
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }
    }
}
