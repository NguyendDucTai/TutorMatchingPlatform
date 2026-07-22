using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Queries.GetPendingProfiles;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/admin/tutor-profiles")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingProfiles([FromQuery] int? subjectId)
        {
            var result = await _sender.Send(new GetPendingProfilesQuery { SubjectId = subjectId });
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveProfile(int id)
        {
            var command = new ApproveTutorProfileCommand { TutorProfileId = id };
            var result = await _sender.Send(command);
            return Ok(new { Success = result });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectProfile(int id, [FromBody] RejectProfileRequest request)
        {
            var command = new RejectTutorProfileCommand 
            { 
                TutorProfileId = id, 
                Reason = request.Reason 
            };
            var result = await _sender.Send(command);
            return Ok(new { Success = result });
        }
    }

    public class RejectProfileRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
