using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile;
using TutorMatchingPlatform.Application.Users.Queries.GetMyProfile;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ISender _sender;

        public UserController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(new GetMyProfileQuery { UserId = userId });
            
            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(result);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromForm] string fullName,
            [FromForm] string? bio,
            [FromForm] string? studyGoals,
            [FromForm] string? targetSubjectsJson,
            IFormFile? avatarFile)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var command = new UpdateMyProfileCommand
            {
                UserId = userId,
                FullName = fullName,
                Bio = bio,
                StudyGoals = studyGoals,
                TargetSubjectsJson = targetSubjectsJson
            };

            if (avatarFile != null)
            {
                command.AvatarFile = new FileUploadDto
                {
                    Content = avatarFile.OpenReadStream(),
                    FileName = avatarFile.FileName,
                    ContentType = avatarFile.ContentType
                };
            }

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }
    }
}
