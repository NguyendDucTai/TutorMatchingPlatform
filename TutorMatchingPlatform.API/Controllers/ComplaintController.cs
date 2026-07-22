using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorMatchingPlatform.Application.Complaints.Commands.SubmitComplaint;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/complaints")]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly ISender _sender;

        public ComplaintController(ISender sender)
        {
            _sender = sender;
        }

        // UC-12: Submit Complaint
        [HttpPost]
        public async Task<IActionResult> SubmitComplaint([FromBody] SubmitComplaintRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int reporterId))
            {
                return Unauthorized();
            }

            var command = new SubmitComplaintCommand
            {
                ReporterId = reporterId,
                ReportedUserId = request.ReportedUserId,
                SessionId = request.SessionId,
                Type = request.Type,
                Description = request.Description
            };

            var result = await _sender.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }
    }

    public class SubmitComplaintRequestDto
    {
        public int ReportedUserId { get; set; }
        public int? SessionId { get; set; }
        public ComplaintType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
