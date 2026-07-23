using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Queries.GetPendingProfiles;
using TutorMatchingPlatform.Application.Complaints.Queries.GetPendingComplaints;
using TutorMatchingPlatform.Application.Complaints.Commands.ResolveComplaint;
using TutorMatchingPlatform.Application.Credits.Queries.GetPendingCreditRequests;
using TutorMatchingPlatform.Application.Credits.Commands.ApproveCreditRequest;
using TutorMatchingPlatform.Application.Credits.Commands.RejectCreditRequest;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("tutor-profiles/pending")]
        public async Task<IActionResult> GetPendingProfiles([FromQuery] int? subjectId)
        {
            var result = await _sender.Send(new GetPendingProfilesQuery { SubjectId = subjectId });
            return Ok(result);
        }

        [HttpPost("tutor-profiles/{id}/approve")]
        public async Task<IActionResult> ApproveProfile(int id)
        {
            var command = new ApproveTutorProfileCommand { TutorProfileId = id };
            var result = await _sender.Send(command);
            return Ok(new { Success = result });
        }

        [HttpPost("tutor-profiles/{id}/reject")]
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

        // UC-14: Review and Resolve Complaint
        [HttpGet("complaints/pending")]
        public async Task<IActionResult> GetPendingComplaints()
        {
            var query = new GetPendingComplaintsQuery();
            var complaints = await _sender.Send(query);
            return Ok(complaints);
        }

        [HttpPost("complaints/{id}/resolve")]
        public async Task<IActionResult> ResolveComplaint(int id, [FromBody] ResolveComplaintRequestDto request)
        {
            var command = new ResolveComplaintCommand
            {
                ComplaintId = id,
                Action = request.Action,
                Reason = request.Reason,
                SuspendDays = request.SuspendDays
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }
        [HttpGet("credits/pending")]
        public async Task<IActionResult> GetPendingCreditRequests()
        {
            var query = new GetPendingCreditRequestsQuery();
            var requests = await _sender.Send(query);
            return Ok(requests);
        }

        [HttpPost("credits/{id}/approve")]
        public async Task<IActionResult> ApproveCreditRequest(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdString, out int adminId)) return Unauthorized();

            var command = new ApproveCreditRequestCommand { CreditRequestId = id, AdminUserId = adminId };
            var result = await _sender.Send(command);

            if (!result.Success) return BadRequest(new { Message = result.Message });
            return Ok(result);
        }

        [HttpPost("credits/{id}/reject")]
        public async Task<IActionResult> RejectCreditRequest(int id, [FromBody] RejectCreditRequestDto request)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdString, out int adminId)) return Unauthorized();

            var command = new RejectCreditRequestCommand 
            { 
                CreditRequestId = id, 
                AdminUserId = adminId,
                Reason = request.Reason
            };
            var result = await _sender.Send(command);

            if (!result.Success) return BadRequest(new { Message = result.Message });
            return Ok(result);
        }
    }

    public class RejectCreditRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class RejectProfileRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ResolveComplaintRequestDto
    {
        public ComplaintAction Action { get; set; }
        public string? Reason { get; set; }
        public int? SuspendDays { get; set; }
    }
}
