using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Interfaces;
using TutorMatchingPlatform.Infrastructure.Models;
using TutorMatchingPlatform.Infrastructure.Persistence;

namespace TutorMatchingPlatform.Infrastructure.Repositories
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AvailabilityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Availability>> GetByUserIdAsync(Guid userId)
        {
            var dataModels = await _dbContext.Availabilities
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var list = new List<Availability>();
            foreach (var dm in dataModels)
            {
                var availability = dm.IsRecurring 
                    ? new Availability(dm.UserId, (DayOfWeek)dm.DayOfWeek!, dm.StartTime, dm.EndTime)
                    : new Availability(dm.UserId, dm.SpecificDate!.Value, dm.StartTime, dm.EndTime);

                var type = typeof(Availability);
                type.GetProperty("Id")?.SetValue(availability, dm.Id);
                list.Add(availability);
            }

            return list;
        }

        public async Task BulkReplaceAsync(Guid userId, List<Availability> newAvailabilities)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Delete old ones
                var oldAvailabilities = await _dbContext.Availabilities.Where(a => a.UserId == userId).ToListAsync();
                _dbContext.Availabilities.RemoveRange(oldAvailabilities);

                // Add new ones
                var newDataModels = newAvailabilities.Select(a => new AvailabilityDataModel
                {
                    Id = a.Id == Guid.Empty ? Guid.NewGuid() : a.Id,
                    UserId = a.UserId,
                    DayOfWeek = a.DayOfWeek.HasValue ? (int)a.DayOfWeek.Value : null,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    IsRecurring = a.IsRecurring,
                    SpecificDate = a.SpecificDate.HasValue ? a.SpecificDate.Value : null,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _dbContext.Availabilities.AddRangeAsync(newDataModels);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> HasConflictingBookingsAsync(Guid userId, List<Availability> newAvailabilities)
        {
            var activeBookings = await _dbContext.Bookings
                .Where(b => b.TutorId == userId && 
                            (b.Status == 0 || // Pending
                             b.Status == 1 || // Confirmed
                             b.Status == 4))  // Rescheduled
                .ToListAsync();

            foreach (var b in activeBookings)
            {
                var bookingStart = b.ScheduledStartAt.TimeOfDay;
                var bookingEnd = b.ScheduledEndAt.TimeOfDay;
                var bookingDay = b.ScheduledStartAt.DayOfWeek;
                var bookingDate = b.ScheduledStartAt.Date;

                // Check if this booking is covered by any of the new availability slots
                var isCovered = newAvailabilities.Any(a =>
                {
                    if (a.IsRecurring)
                    {
                        return a.DayOfWeek == bookingDay &&
                               a.StartTime <= bookingStart &&
                               a.EndTime >= bookingEnd;
                    }
                    else
                    {
                        return a.SpecificDate.HasValue &&
                               a.SpecificDate.Value.Date == bookingDate &&
                               a.StartTime <= bookingStart &&
                               a.EndTime >= bookingEnd;
                    }
                });

                if (!isCovered)
                {
                    // Found an active booking that is no longer covered by the tutor's new availability
                    return true;
                }
            }

            return false;
        }
    }
}
