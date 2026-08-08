using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Contracts.Admin;
using TutorPlatform.Application.Features.Admin.Commands.RejectTutor;
using TutorPlatform.Application.Features.Admin.Queries.GetAdminDashboard;
using TutorPlatform.Application.Features.Admin.Queries.GetPendingTutors;
using TutorPlatform.Application.Features.Tutors.Commands.ApproveTutor;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly TutorPlatform.Infrastructure.Persistence.ApplicationDbContext _dbContext;

        public AdminController(IMediator mediator, TutorPlatform.Infrastructure.Persistence.ApplicationDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId;
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var query = new GetAdminDashboardQuery();
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<AdminDashboardDto>.Ok(response));
        }

        [HttpGet("revenue-report")]
        [ProducesResponseType(typeof(ApiResponse<AdminRevenueReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueReport(
            [FromServices] TutorPlatform.Domain.Interfaces.IAdminRepository adminRepository,
            [FromQuery] int? year = null, 
            [FromQuery] string filterType = "month", 
            [FromQuery] string sortBy = "date_asc")
        {
            var report = await adminRepository.GetRevenueReportAsync(year, filterType, sortBy);
            
            var dto = new AdminRevenueReportDto
            {
                TotalRevenue = report.TotalRevenue,
                Periods = report.Periods.Select(p => new AdminRevenuePeriodDto
                {
                    PeriodName = p.PeriodName,
                    Revenue = p.Revenue,
                    TransactionCount = p.TransactionCount
                }).ToList()
            };

            return Ok(ApiResponse<AdminRevenueReportDto>.Ok(dto));
        }

        [HttpGet("pending-tutors")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PendingTutorDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingTutors([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetPendingTutorsQuery
            {
                PageNumber = pageNumber <= 0 ? 1 : pageNumber,
                PageSize = pageSize <= 0 ? 10 : pageSize
            };
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<PendingTutorDto>>.Ok(response));
        }

        [HttpPut("tutors/{id}/approve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveTutor(Guid id)
        {
            Guid adminId;
            try
            {
                adminId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(ApiResponse<object>.Error(401, "Unauthorized admin access."));
            }

            var command = new ApproveTutorCommand
            {
                TutorUserId = id,
                AdminUserId = adminId
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("tutors/{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectTutor(Guid id)
        {
            var command = new RejectTutorCommand(id);
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        // ── User Management ──────────────────────────────────────────────

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? role = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new Application.Features.Admin.Queries.GetAllUsers.GetAllUsersQuery
            {
                PageNumber = pageNumber < 1 ? 1 : pageNumber,
                PageSize = pageSize < 1 ? 20 : pageSize,
                Search = search,
                Role = role,
                IsActive = isActive
            };
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<Domain.Common.PagedResult<Domain.Common.AdminUserResult>>.Ok(response));
        }

        [HttpPut("users/{id}/lock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> LockUser(Guid id)
        {
            var command = new Application.Features.Admin.Commands.LockUser.LockUserCommand { UserId = id };
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("users/{id}/unlock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlockUser(Guid id)
        {
            var command = new Application.Features.Admin.Commands.UnlockUser.UnlockUserCommand { UserId = id };
            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        public class UpdateNoteRequest
        {
            public string? Note { get; set; }
        }

        [HttpPut("users/{id}/note")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserNote(
            Guid id, 
            [FromBody] UpdateNoteRequest request, 
            [FromServices] TutorPlatform.Domain.Interfaces.IAdminRepository adminRepository,
            [FromServices] TutorPlatform.Application.Contracts.Notifications.INotificationSender notificationSender)
        {
            var response = await adminRepository.UpdateUserNoteAsync(id, request.Note);
            
            if (response && !string.IsNullOrWhiteSpace(request.Note))
            {
                var title = "[WARNING] Cảnh cáo từ Trung tâm!";
                var message = request.Note;

                var notification = new TutorPlatform.Infrastructure.Models.NotificationDataModel
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    Title = title,
                    Message = message,
                    Type = 7, // System
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Notifications.AddAsync(notification);
                await _dbContext.SaveChangesAsync();

                var dto = new TutorPlatform.Application.Contracts.Notifications.NotificationDto
                {
                    Id = notification.Id,
                    UserId = id,
                    Title = title,
                    Message = message,
                    Type = "System",
                    IsRead = false,
                    CreatedAt = notification.CreatedAt
                };

                await notificationSender.SendNotificationAsync(id, dto);
            }

            return Ok(ApiResponse<bool>.Ok(response));
        }

        // ── Review Management ─────────────────────────────────────────────

        [HttpGet("reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? reviewType = null,
            [FromQuery] int? rating = null)
        {
            var query = new Application.Features.Admin.Queries.GetAllReviews.GetAllReviewsQuery
            {
                PageNumber = pageNumber < 1 ? 1 : pageNumber,
                PageSize = pageSize < 1 ? 20 : pageSize,
                Search = search,
                ReviewType = reviewType,
                Rating = rating
            };
            var response = await _mediator.Send(query);
            return Ok(ApiResponse<Domain.Common.PagedResult<Domain.Common.AdminReviewResult>>.Ok(response));
        }

        [HttpGet("deposit-requests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepositRequests(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var baseQuery = _dbContext.DepositRequests.AsQueryable();

            if (startDate.HasValue)
            {
                baseQuery = baseQuery.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(r => r.CreatedAt <= endOfDay);
            }

            var query = baseQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var items = await query.Select(r => new {
                r.Id,
                r.UserId,
                r.Amount,
                r.Status,
                r.CreatedAt,
                RequesterName = r.User.FullName,
                RequesterEmail = r.User.Email,
                RequesterRole = r.User.Role == 2 ? "Student" : r.User.Role == 1 ? "Tutor" : "Admin"
            }).ToListAsync();

            var totalCount = await baseQuery.CountAsync();

            var pagedResult = new {
                items,
                totalCount
            };

            return Ok(ApiResponse<object>.Ok(pagedResult));
        }

        [HttpPut("deposit-requests/{id}/approve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveDepositRequest(Guid id)
        {
            var request = await _dbContext.DepositRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(ApiResponse<object>.Error(404, "Yêu cầu nạp tiền không tồn tại."));
            }

            if (request.Status != 0)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Yêu cầu này đã được xử lý từ trước."));
            }

            // Update user balance
            var user = await _dbContext.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Error(404, "Người dùng không tồn tại."));
            }

            user.CreditBalance += request.Amount;
            request.Status = 1; // Approved
            request.UpdatedAt = DateTime.UtcNow;

            // Create transaction log
            var tx = new TutorPlatform.Infrastructure.Models.CreditTransactionDataModel
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Type = 0, // Credit
                Description = $"Admin approved deposit of {request.Amount:N2} credits.",
                BalanceAfter = user.CreditBalance,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.CreditTransactions.AddAsync(tx);
            await _dbContext.SaveChangesAsync();

            await _mediator.Publish(new TutorPlatform.Application.Features.Notifications.Events.DepositRequestApprovedEvent
            {
                UserId = request.UserId,
                Amount = request.Amount,
                NewBalance = user.CreditBalance
            });

            return Ok(ApiResponse<bool>.Ok(true));
        }

        [HttpPut("deposit-requests/{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectDepositRequest(Guid id)
        {
            var request = await _dbContext.DepositRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound(ApiResponse<object>.Error(404, "Yêu cầu nạp tiền không tồn tại."));
            }

            if (request.Status != 0)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Yêu cầu này đã được xử lý từ trước."));
            }

            request.Status = 2; // Rejected
            request.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}
