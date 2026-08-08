using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            var dm = await _dbContext.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (dm == null) return null;
            return MapToDomain(dm);
        }

        public async Task<IReadOnlyList<Booking>> GetByTutorIdAsync(Guid tutorId)
        {
            var dataModels = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.TutorId == tutorId)
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<IReadOnlyList<Booking>> GetByStudentIdAsync(Guid studentId)
        {
            var dataModels = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.StudentId == studentId)
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<PagedResult<Booking>> GetPagedAsync(Guid userId, UserRole role, int pageNumber, int pageSize)
        {
            var query = _dbContext.Bookings.AsNoTracking();
            if (role == UserRole.Tutor)
            {
                query = query.Where(b => b.TutorId == userId);
            }
            else if (role == UserRole.Student)
            {
                query = query.Where(b => b.StudentId == userId);
            }

            var totalCount = await query.CountAsync();
            var dataModels = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = dataModels.Select(MapToDomain).ToList();
            return new PagedResult<Booking>(items, totalCount);
        }

        public async Task<Booking> AddAsync(Booking booking)
        {
            var dm = new BookingDataModel
            {
                Id = booking.Id,
                TutorId = booking.TutorId,
                StudentId = booking.StudentId,
                SubjectId = booking.SubjectId,
                ScheduledStartAt = booking.ScheduledStartAt,
                ScheduledEndAt = booking.ScheduledEndAt,
                Status = (int)booking.Status,
                MeetingLink = booking.MeetingLink,
                Notes = booking.Notes,
                CreditAmount = booking.CreditAmount,
                CancellationReason = booking.CancellationReason,
                CancelledBy = booking.CancelledBy,
                CancelledAt = booking.CancelledAt,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };

            await _dbContext.Bookings.AddAsync(dm);
            await _dbContext.SaveChangesAsync();

            return booking;
        }

        public async Task UpdateAsync(Booking booking)
        {
            var dm = await _dbContext.Bookings.FindAsync(booking.Id);
            if (dm != null)
            {
                dm.ScheduledStartAt = booking.ScheduledStartAt;
                dm.ScheduledEndAt = booking.ScheduledEndAt;
                dm.Status = (int)booking.Status;
                dm.MeetingLink = booking.MeetingLink;
                dm.Notes = booking.Notes;
                dm.CancellationReason = booking.CancellationReason;
                dm.CancelledBy = booking.CancelledBy;
                dm.CancelledAt = booking.CancelledAt;
                dm.UpdatedAt = booking.UpdatedAt;

                _dbContext.Bookings.Update(dm);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> HasOverlappingBookingAsync(Guid tutorId, DateTime start, DateTime end)
        {
            return await _dbContext.Bookings
                .AnyAsync(b => b.TutorId == tutorId 
                            && (b.Status == (int)BookingStatus.Pending || b.Status == (int)BookingStatus.Confirmed)
                            && b.ScheduledStartAt < end 
                            && b.ScheduledEndAt > start);
        }

        /// <summary>
        /// Checks if the tutor has a CONFIRMED booking whose time range overlaps with [start, end).
        /// Pending bookings are intentionally excluded to allow multiple students to register
        /// for the same time slot (group class). Only a confirmed class can block a new booking.
        /// </summary>
        public async Task<bool> HasConfirmedOverlappingBookingAsync(Guid tutorId, DateTime start, DateTime end)
        {
            return await _dbContext.Bookings
                .AnyAsync(b => b.TutorId == tutorId
                            && b.Status == (int)BookingStatus.Confirmed
                            && b.ScheduledStartAt < end
                            && b.ScheduledEndAt > start);
        }

        public async Task<bool> HasStudentConfirmedOverlappingBookingAsync(Guid studentId, DateTime start, DateTime end)
        {
            return await _dbContext.Bookings
                .AnyAsync(b => b.StudentId == studentId
                            && b.Status == (int)BookingStatus.Confirmed
                            && b.ScheduledStartAt < end
                            && b.ScheduledEndAt > start);
        }

        public async Task<bool> HasStudentActiveBookingForSubjectAsync(Guid studentId, DateTime start, DateTime end, Guid subjectId)
        {
            return await _dbContext.Bookings
                .AnyAsync(b => b.StudentId == studentId
                            && b.SubjectId == subjectId
                            && (b.Status == (int)BookingStatus.Pending || b.Status == (int)BookingStatus.Confirmed)
                            && b.ScheduledStartAt < end
                            && b.ScheduledEndAt > start);
        }




        private Booking MapToDomain(BookingDataModel dm)
        {
            var booking = new Booking(dm.TutorId, dm.StudentId, dm.SubjectId, dm.ScheduledStartAt, dm.ScheduledEndAt, dm.CreditAmount, dm.Notes);
            
            var type = typeof(Booking);
            type.GetProperty("Id")?.SetValue(booking, dm.Id);
            
            if (dm.Status == (int)BookingStatus.Confirmed) booking.Confirm(dm.MeetingLink);
            if (dm.Status == (int)BookingStatus.Completed) 
            {
                booking.Confirm(dm.MeetingLink); // to pass state rules
                booking.Complete();
            }
            if (dm.Status == (int)BookingStatus.Cancelled)
            {
                type.GetProperty("Status")?.SetValue(booking, BookingStatus.Cancelled);
                type.GetProperty("CancelledBy")?.SetValue(booking, dm.CancelledBy);
                type.GetProperty("CancellationReason")?.SetValue(booking, dm.CancellationReason);
                type.GetProperty("CancelledAt")?.SetValue(booking, dm.CancelledAt);
            }
            
            return booking;
        }
    }
}
