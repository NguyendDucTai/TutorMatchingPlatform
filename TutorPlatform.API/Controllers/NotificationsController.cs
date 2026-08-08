using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Domain.Common;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Application.Features.Notifications.Commands.MarkAllAsRead;
using TutorPlatform.Application.Features.Notifications.Commands.MarkAsRead;
using TutorPlatform.Application.Features.Notifications.Queries.GetNotifications;
using TutorPlatform.Application.Features.Notifications.Queries.GetUnreadCount;

namespace TutorPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User is not authenticated properly.");
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetNotificationsQuery
            {
                UserId = GetUserId(),
                PageNumber = pageNumber <= 0 ? 1 : pageNumber,
                PageSize = pageSize <= 0 ? 10 : pageSize
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(response));
        }

        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var query = new GetUnreadCountQuery
            {
                UserId = GetUserId()
            };

            var response = await _mediator.Send(query);
            return Ok(ApiResponse<int>.Ok(response));
        }

        [HttpPut("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var command = new MarkAsReadCommand
            {
                NotificationId = id,
                UserId = GetUserId()
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }

        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var command = new MarkAllAsReadCommand
            {
                UserId = GetUserId()
            };

            var response = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(response));
        }
    }
}
