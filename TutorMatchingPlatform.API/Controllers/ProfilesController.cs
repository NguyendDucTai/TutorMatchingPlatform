using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Users.Queries.GetMyProfile;
using TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile;
using TutorMatchingPlatform.Application.Profiles.Commands.UpdateStudentProfile;
using TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorProfile;
using TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorSubjects;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly ISender _sender;

        public ProfilesController(ISender sender)
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
                FullName = fullName
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

        [HttpPut("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileCommand command)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            command.UserId = userId;
            var result = await _sender.Send(command);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update student profile." });
            }

            return Ok(new { Success = true });
        }

        [HttpPut("tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> UpdateTutorProfile(
            [FromForm] string? bio,
            [FromForm] string? qualificationsText,
            [FromForm] IFormFileCollection qualificationFiles)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var command = new UpdateTutorProfileCommand
            {
                UserId = userId,
                Bio = bio,
                QualificationsText = qualificationsText
            };

            if (qualificationFiles != null && qualificationFiles.Count > 0)
            {
                foreach (var file in qualificationFiles)
                {
                    command.QualificationFiles.Add(new FileUploadDto
                    {
                        Content = file.OpenReadStream(),
                        FileName = file.FileName,
                        ContentType = file.ContentType
                    });
                }
            }

            var result = await _sender.Send(command);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update tutor profile." });
            }

            return Ok(new { Success = true });
        }

        [HttpPost("tutor/subjects")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> UpdateTutorSubjects([FromBody] List<SubjectRateDto> subjects)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var command = new UpdateTutorSubjectsCommand
            {
                UserId = userId,
                Subjects = subjects
            };

            var result = await _sender.Send(command);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update tutor subjects." });
            }

            return Ok(new { Success = true });
        }
    }
}
