using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Profiles;
using TutorPlatform.Application.Features.Profiles.Commands.UpdateStudentProfile;
using TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorProfile;
using TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorSubjects;
using TutorPlatform.Application.Features.Profiles.Commands.UpdateUserProfile;
using TutorPlatform.Application.Features.Profiles.Queries.GetMyProfile;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid? GetUserIdNullable()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("nameid")
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return null;
            }
            return userId;
        }

        private Guid GetUserId()
        {
            var userId = GetUserIdNullable();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId.Value;
        }

        private int GetUserRole()
        {
            var roleString = User.FindFirstValue(ClaimTypes.Role)
                             ?? User.FindFirstValue("role")
                             ?? User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (string.IsNullOrEmpty(roleString)) return -1;

            if (Enum.TryParse<TutorPlatform.Domain.Enums.UserRole>(roleString, true, out var role))
            {
                return (int)role;
            }
            if (int.TryParse(roleString, out var roleInt))
            {
                return roleInt;
            }
            return -1;
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<MyProfileResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserIdNullable();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<MyProfileResponse>.Error(401, "Phiên đăng nhập hết hạn hoặc chưa xác thực."));
            }

            var query = new GetMyProfileQuery { UserId = userId.Value };
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<MyProfileResponse>.Ok(response));
        }

        [HttpPut("me")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileCommand command)
        {
            command.UserId = GetUserId();
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("tutor")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTutorProfile([FromBody] UpdateTutorProfileCommand command)
        {
            if (GetUserRole() != 1) // 1 = Tutor
            {
                return Forbid();
            }

            command.UserId = GetUserId();
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("student")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileCommand command)
        {
            if (GetUserRole() != 2) // 2 = Student
            {
                return Forbid();
            }

            command.UserId = GetUserId();
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPost("tutor/subjects")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTutorSubjects([FromBody] UpdateTutorSubjectsCommand command)
        {
            if (GetUserRole() != 1) // 1 = Tutor
            {
                return Forbid();
            }

            command.UserId = GetUserId();
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }
    }
}
