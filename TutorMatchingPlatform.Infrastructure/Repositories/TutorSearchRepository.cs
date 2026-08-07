using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Interfaces;
using TutorMatchingPlatform.Infrastructure.Persistence;

namespace TutorMatchingPlatform.Infrastructure.Repositories
{
    public class TutorSearchRepository : ITutorSearchRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TutorSearchRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<TutorSearchResult>> SearchTutorsAsync(
            Guid? subjectId,
            decimal? minPrice,
            decimal? maxPrice,
            DayOfWeek? dayOfWeek,
            DateTime? specificDate,
            TimeSpan? startTime,
            TimeSpan? endTime,
            string? searchQuery,
            string? sortBy,
            int pageNumber,
            int pageSize,
            decimal? minRating = null)
        {
            var query = _dbContext.TutorProfiles
                .AsNoTracking()
                .Include(tp => tp.User)
                .Include(tp => tp.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
                .Where(tp => tp.IsApproved);

            // 1. Text Search Query (Name, Bio, Qualifications)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(tp => 
                    tp.User.FullName.Contains(searchQuery) || 
                    (tp.Bio != null && tp.Bio.Contains(searchQuery)) || 
                    (tp.Qualifications != null && tp.Qualifications.Contains(searchQuery)));
            }

            // 2. Subject Filtering
            if (subjectId.HasValue)
            {
                query = query.Where(tp => tp.TutorSubjects.Any(ts => ts.SubjectId == subjectId.Value));
            }

            // 3. Price Filtering
            if (minPrice.HasValue)
            {
                if (subjectId.HasValue)
                {
                    query = query.Where(tp => tp.TutorSubjects.Any(ts => ts.SubjectId == subjectId.Value && ts.HourlyCredits >= minPrice.Value));
                }
                else
                {
                    query = query.Where(tp => tp.TutorSubjects.Any(ts => ts.HourlyCredits >= minPrice.Value));
                }
            }

            if (maxPrice.HasValue)
            {
                if (subjectId.HasValue)
                {
                    query = query.Where(tp => tp.TutorSubjects.Any(ts => ts.SubjectId == subjectId.Value && ts.HourlyCredits <= maxPrice.Value));
                }
                else
                {
                    query = query.Where(tp => tp.TutorSubjects.Any(ts => ts.HourlyCredits <= maxPrice.Value));
                }
            }

            // 4. Availability & Conflict Matching
            if (startTime.HasValue && endTime.HasValue)
            {
                if (specificDate.HasValue)
                {
                    var targetDate = specificDate.Value.Date;
                    var targetDayOfWeek = specificDate.Value.DayOfWeek;

                    // Tutor must be available in that slot (either recurring on that day-of-week OR specific date)
                    query = query.Where(tp => _dbContext.Availabilities.Any(a =>
                        a.UserId == tp.UserId &&
                        (
                            (a.IsRecurring && a.DayOfWeek == (int)targetDayOfWeek && a.StartTime <= startTime.Value && a.EndTime >= endTime.Value)
                            ||
                            (!a.IsRecurring && a.SpecificDate == targetDate && a.StartTime <= startTime.Value && a.EndTime >= endTime.Value)
                        )
                    ));

                    // And tutor must NOT have a booking conflict on that day & time slot
                    var startDateTime = targetDate.Add(startTime.Value);
                    var endDateTime = targetDate.Add(endTime.Value);

                    query = query.Where(tp => !_dbContext.Bookings.Any(b =>
                        b.TutorId == tp.UserId &&
                        b.Status != (int)Domain.Enums.BookingStatus.Cancelled &&
                        b.ScheduledStartAt < endDateTime &&
                        b.ScheduledEndAt > startDateTime
                    ));
                }
                else if (dayOfWeek.HasValue)
                {
                    // Only day of week is requested (recurring matching only, no date booking check)
                    query = query.Where(tp => _dbContext.Availabilities.Any(a =>
                        a.UserId == tp.UserId &&
                        a.IsRecurring &&
                        a.DayOfWeek == (int)dayOfWeek.Value &&
                        a.StartTime <= startTime.Value &&
                        a.EndTime >= endTime.Value
                    ));
                }
            }

            // 4.5. Rating Filtering
            if (minRating.HasValue)
            {
                query = query.Where(tp => (tp.TotalReviews > 0 ? tp.AverageRating : 5.0M) >= minRating.Value);
            }

            // 5. Sorting
            switch (sortBy?.ToLower())
            {
                case "price_asc":
                    if (subjectId.HasValue)
                    {
                        query = query.OrderBy(tp => tp.TutorSubjects.Where(ts => ts.SubjectId == subjectId.Value).Min(ts => ts.HourlyCredits));
                    }
                    else
                    {
                        query = query.OrderBy(tp => tp.TutorSubjects.Min(ts => ts.HourlyCredits));
                    }
                    break;

                case "price_desc":
                    if (subjectId.HasValue)
                    {
                        query = query.OrderByDescending(tp => tp.TutorSubjects.Where(ts => ts.SubjectId == subjectId.Value).Max(ts => ts.HourlyCredits));
                    }
                    else
                    {
                        query = query.OrderByDescending(tp => tp.TutorSubjects.Max(ts => ts.HourlyCredits));
                    }
                    break;

                case "reviews_desc":
                    query = query.OrderByDescending(tp => tp.TotalReviews);
                    break;

                case "sessions_desc":
                    query = query.OrderByDescending(tp => tp.TotalSessions);
                    break;

                case "rating_desc":
                default:
                    query = query.OrderByDescending(tp => tp.TotalReviews > 0 ? tp.AverageRating : 5.0M)
                                 .ThenByDescending(tp => tp.TotalReviews);
                    break;
            }

            // 6. Pagination & Selection
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(tp => new TutorSearchResult
                {
                    TutorId = tp.UserId,
                    FullName = tp.User.FullName,
                    AvatarUrl = tp.User.AvatarUrl,
                    Bio = tp.Bio,
                    Qualifications = tp.Qualifications,
                    AverageRating = tp.AverageRating,
                    TotalReviews = tp.TotalReviews,
                    TotalSessions = tp.TotalSessions,
                    Phone = tp.User.Phone,
                    Email = tp.User.Email,
                    DefaultMeetingLink = tp.DefaultMeetingLink,
                    Subjects = tp.TutorSubjects.Select(ts => new TutorSearchSubjectResult
                    {
                        SubjectId = ts.SubjectId,
                        SubjectName = ts.Subject.Name,
                        ProficiencyLevel = ts.ProficiencyLevel,
                        HourlyCredits = ts.HourlyCredits
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResult<TutorSearchResult>(items, totalCount);
        }

        public async Task<bool> ApproveTutorAsync(Guid tutorUserId, Guid adminUserId)
        {
            var profile = await _dbContext.TutorProfiles.FirstOrDefaultAsync(tp => tp.UserId == tutorUserId);
            if (profile == null) return false;

            profile.IsApproved = true;
            profile.ApprovalStatus = 1;
            profile.ApprovedAt = DateTime.UtcNow;
            profile.ApprovedBy = adminUserId;
            profile.UpdatedAt = DateTime.UtcNow;

            _dbContext.TutorProfiles.Update(profile);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<TutorSearchResult?> GetTutorDetailAsync(Guid tutorUserId)
        {
            var tp = await _dbContext.TutorProfiles
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(p => p.UserId == tutorUserId);

            if (tp == null) return null;

            return new TutorSearchResult
            {
                TutorId = tp.UserId,
                FullName = tp.User.FullName,
                AvatarUrl = tp.User.AvatarUrl,
                Bio = tp.Bio,
                Qualifications = tp.Qualifications,
                AverageRating = tp.AverageRating,
                TotalReviews = tp.TotalReviews,
                TotalSessions = tp.TotalSessions,
                Phone = tp.User.Phone,
                Email = tp.User.Email,
                DefaultMeetingLink = tp.DefaultMeetingLink,
                Subjects = tp.TutorSubjects.Select(ts => new TutorSearchSubjectResult
                {
                    SubjectId = ts.SubjectId,
                    SubjectName = ts.Subject.Name,
                    ProficiencyLevel = ts.ProficiencyLevel,
                    HourlyCredits = ts.HourlyCredits
                }).ToList()
            };
        }
    }
}
