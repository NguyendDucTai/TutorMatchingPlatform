using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Infrastructure.Data;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly TutorMatchingPlatformDbContext _context;

        public NotificationsController(TutorMatchingPlatformDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(item => item.ReceiverId == userId)
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => new
                {
                    item.Id,
                    item.ReceiverId,
                    item.Title,
                    item.Message,
                    item.IsWarning,
                    item.IsRead,
                    item.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var notification = await _context.Notifications
                .SingleOrDefaultAsync(item => item.Id == id && item.ReceiverId == userId);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(item => item.ReceiverId == userId && !item.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
                notification.IsRead = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }
    }
}
