using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.API.Common;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IAppDbContext _context;

        public AvailabilitiesController(IAppDbContext context)
        {
            _context = context;
        }

        [HttpGet("tutor/{tutorProfileId}")]
        public async Task<IActionResult> GetTutorAvailability(int tutorProfileId)
        {
            var availabilities = await _context.Availabilities
                .AsNoTracking()
                .Where(a => a.TutorProfileId == tutorProfileId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.TutorProfileId,
                    a.DayOfWeek,
                    a.StartTime,
                    a.EndTime,
                    a.IsRecurring,
                    a.SpecificDate
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(availabilities));
        }

        [HttpPost]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> CreateAvailability([FromBody] CreateAvailabilityDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var tutorProfile = await _context.TutorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tutorProfile == null)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Tutor profile not found."));
            }

            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(ApiResponse<object>.Error(400, "StartTime must be before EndTime."));
            }

            var availability = new Availability
            {
                TutorProfileId = tutorProfile.Id,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsRecurring = request.IsRecurring,
                SpecificDate = request.SpecificDate?.Date
            };

            await _context.Availabilities.AddAsync(availability);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Created(availability));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var tutorProfile = await _context.TutorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tutorProfile == null)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Tutor profile not found."));
            }

            var availability = await _context.Availabilities.FirstOrDefaultAsync(a => a.Id == id && a.TutorProfileId == tutorProfile.Id);
            if (availability == null)
            {
                return NotFound(ApiResponse<object>.Error(404, "Availability slot not found."));
            }

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true));
        }
    }

    public class CreateAvailabilityDto
    {
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsRecurring { get; set; } = true;
        public DateTime? SpecificDate { get; set; }
    }
}
