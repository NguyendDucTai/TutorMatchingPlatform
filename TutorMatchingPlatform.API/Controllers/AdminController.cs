using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile;
using TutorMatchingPlatform.Application.TutorProfiles.Queries.GetPendingProfiles;
using TutorMatchingPlatform.Application.Complaints.Queries.GetPendingComplaints;
using TutorMatchingPlatform.Application.Complaints.Commands.ResolveComplaint;
using TutorMatchingPlatform.Application.Credits.Queries.GetPendingCreditRequests;
using TutorMatchingPlatform.Application.Credits.Commands.ApproveCreditRequest;
using TutorMatchingPlatform.Application.Credits.Commands.RejectCreditRequest;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Infrastructure.Data;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly TutorMatchingPlatformDbContext _context;
        private readonly TutorMatchingPlatform.Application.Interfaces.INotificationSender _notificationSender;

        public AdminController(
            ISender sender,
            TutorMatchingPlatformDbContext context,
            TutorMatchingPlatform.Application.Interfaces.INotificationSender notificationSender)
        {
            _sender = sender;
            _context = context;
            _notificationSender = notificationSender;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string timeRange = "30days", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var query = new TutorMatchingPlatform.Application.Admin.Queries.GetDashboardStatistics.GetDashboardStatisticsQuery
            {
                TimeRange = timeRange,
                CustomStartDate = startDate,
                CustomEndDate = endDate
            };
            var result = await _sender.Send(query);
            return Ok(result);
        }

        [HttpGet("tutors")]
        public async Task<IActionResult> GetTutors([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _sender.Send(new TutorMatchingPlatform.Application.TutorProfiles.Queries.GetAllTutors.GetAllTutorsQuery { PageNumber = pageNumber, PageSize = pageSize });
            return Ok(result);
        }

        [HttpGet("tutor-profiles/pending")]
        public async Task<IActionResult> GetPendingProfiles()
        {
            var result = await _sender.Send(new GetPendingProfilesQuery());
            return Ok(result);
        }

        [HttpPost("tutor-profiles/{id}/approve")]
        public async Task<IActionResult> ApproveProfile(int id)
        {
            try
            {
                var command = new ApproveTutorProfileCommand { TutorProfileId = id };
                var result = await _sender.Send(command);
                return Ok(new { Success = result });
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        [HttpPost("tutor-profiles/{id}/reject")]
        public async Task<IActionResult> RejectProfile(int id, [FromBody] RejectProfileRequest request)
        {
            try
            {
                var command = new RejectTutorProfileCommand
                {
                    TutorProfileId = id,
                    Reason = request.Reason
                };
                var result = await _sender.Send(command);
                return Ok(new { Success = result });
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
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

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Include(user => user.TutorProfile)
                .OrderByDescending(user => user.CreatedAt)
                .Select(user => new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.AvatarUrl,
                    user.IsSuspended,
                    user.CreditBalance,
                    user.CreatedAt,
                    TutorProfile = user.TutorProfile == null ? null : new
                    {
                        user.TutorProfile.Status,
                        user.TutorProfile.ReputationScore
                    }
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("users/{id}/suspend")]
        public async Task<IActionResult> ToggleUserSuspension(int id, [FromBody] UserModerationRequest? request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });
            if (user.Role == UserRole.Administrator)
                return BadRequest(new { Message = "Administrator accounts cannot be suspended here." });

            user.IsSuspended = !user.IsSuspended;
            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.IsSuspended, request?.Reason });
        }

        [HttpPost("users/{id}/kick")]
        public async Task<IActionResult> KickUser(int id, [FromBody] UserModerationRequest? request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });
            if (user.Role == UserRole.Administrator)
                return BadRequest(new { Message = "Administrator accounts cannot be deactivated here." });

            // Keep relational/history data intact while permanently preventing login.
            user.IsSuspended = true;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.IsSuspended, request?.Reason });
        }

        [HttpPut("users/{id}/note")]
        public async Task<IActionResult> UpdateUserNote(int id, [FromBody] UserModerationRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(TutorMatchingPlatform.API.Common.ApiResponse<object>.Error(404, "User not found."));

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                var title = "[WARNING] Cảnh cáo từ Admin!";
                var message = request.Reason;

                var notification = new TutorMatchingPlatform.Domain.Entities.Notification
                {
                    ReceiverId = id,
                    Title = title,
                    Message = message,
                    IsWarning = true,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                await _notificationSender.SendNotificationAsync(id, new
                {
                    notification.Id,
                    UserId = notification.ReceiverId,
                    notification.Title,
                    notification.Message,
                    Type = "Warning",
                    notification.IsRead,
                    notification.CreatedAt
                });
            }

            return Ok(TutorMatchingPlatform.API.Common.ApiResponse<bool>.Ok(true));
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

    public class UserModerationRequest
    {
        public string? Reason { get; set; }
    }
}
