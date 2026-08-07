using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Interfaces;
using TutorMatchingPlatform.Infrastructure.Models;
using TutorMatchingPlatform.Infrastructure.Persistence;

namespace TutorMatchingPlatform.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStats();

            // 1. User stats
            stats.TotalUsers = await _dbContext.Users.CountAsync();
            stats.TotalTutors = await _dbContext.Users.CountAsync(u => u.Role == 1); // 1 = Tutor
            stats.TotalStudents = await _dbContext.Users.CountAsync(u => u.Role == 2); // 2 = Student

            // 2. Pending Tutors count
            stats.PendingTutors = await _dbContext.TutorProfiles.CountAsync(tp => tp.ApprovalStatus == 0 
                && tp.Bio != null && tp.Bio != "" 
                && tp.Qualifications != null && tp.Qualifications != ""
                && _dbContext.Availabilities.Any(a => a.UserId == tp.UserId));

            // 3. Booking stats
            stats.TotalBookings = await _dbContext.Bookings.CountAsync();
            stats.CompletedBookings = await _dbContext.Bookings.CountAsync(b => b.Status == 2); // 2 = Completed
            stats.CancelledBookings = await _dbContext.Bookings.CountAsync(b => b.Status == 3); // 3 = Cancelled

            if (stats.TotalBookings > 0)
            {
                stats.CompletionRate = Math.Round((double)stats.CompletedBookings * 100.0 / stats.TotalBookings, 2);
            }
            else
            {
                stats.CompletionRate = 0.0;
            }

            // 4. Learning Goal stats
            var totalGoals = await _dbContext.LearningGoals.CountAsync();
            var completedGoals = await _dbContext.LearningGoals.CountAsync(g => g.Status == 2); // 2 = Completed

            if (totalGoals > 0)
            {
                stats.GoalCompletionRate = Math.Round((double)completedGoals * 100.0 / totalGoals, 2);
            }
            else
            {
                stats.GoalCompletionRate = 0.0;
            }

            // 5. Popular Subjects (Active only)
            var popularSubjectsData = await _dbContext.Bookings
                .GroupBy(b => b.SubjectId)
                .Select(g => new
                {
                    SubjectId = g.Key,
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToListAsync();

            var subjectIds = popularSubjectsData.Select(x => x.SubjectId).ToList();
            var activeSubjects = await _dbContext.Subjects
                .Where(s => s.IsActive && subjectIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            stats.PopularSubjects = popularSubjectsData
                .Where(x => activeSubjects.ContainsKey(x.SubjectId))
                .Select(x => new AdminPopularSubject
                {
                    SubjectId = x.SubjectId,
                    SubjectName = activeSubjects[x.SubjectId],
                    BookingCount = x.BookingCount
                })
                .ToList();

            // 6. Recent Bookings (Top 5)
            var recentBookingsData = await _dbContext.Bookings
                .Include(b => b.Tutor)
                .Include(b => b.Student)
                .Include(b => b.Subject)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            stats.RecentBookings = recentBookingsData.Select(b => new AdminRecentBooking
            {
                BookingId = b.Id,
                TutorName = b.Tutor.FullName,
                StudentName = b.Student.FullName,
                SubjectName = b.Subject.Name,
                ScheduledStartAt = b.ScheduledStartAt,
                ScheduledEndAt = b.ScheduledEndAt,
                CreditAmount = b.CreditAmount,
                Status = MapBookingStatusString(b.Status)
            }).ToList();

            // 7. Total revenue (sum of approved deposit requests)
            stats.TotalRevenue = await _dbContext.DepositRequests
                .Where(r => r.Status == 1) // Approved
                .SumAsync(r => r.Amount);

            // 8. Growth statistics (compared to start of current month)
            var startOfCurrentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            
            var tutorsLastMonth = await _dbContext.Users.CountAsync(u => u.Role == 1 && u.CreatedAt < startOfCurrentMonth);
            var studentsLastMonth = await _dbContext.Users.CountAsync(u => u.Role == 2 && u.CreatedAt < startOfCurrentMonth);
            var bookingsLastMonth = await _dbContext.Bookings.CountAsync(b => b.CreatedAt < startOfCurrentMonth);
            var revenueLastMonth = await _dbContext.DepositRequests
                .Where(r => r.Status == 1 && r.CreatedAt < startOfCurrentMonth)
                .SumAsync(r => (decimal?)r.Amount) ?? 0m;

            stats.TutorsGrowth = CalculateGrowth(stats.TotalTutors, tutorsLastMonth);
            stats.StudentsGrowth = CalculateGrowth(stats.TotalStudents, studentsLastMonth);
            stats.BookingsGrowth = CalculateGrowth(stats.TotalBookings, bookingsLastMonth);
            stats.RevenueGrowth = CalculateGrowth((double)stats.TotalRevenue, (double)revenueLastMonth);

            return stats;
        }

        public async Task<PagedResult<PendingTutorResult>> GetPendingTutorsAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.TutorProfiles
                .Include(tp => tp.User)
                .Where(tp => tp.ApprovalStatus == 0 
                          && tp.Bio != null && tp.Bio != "" 
                          && tp.Qualifications != null && tp.Qualifications != ""
                          && _dbContext.Availabilities.Any(a => a.UserId == tp.UserId))
                .OrderByDescending(tp => tp.CreatedAt);

            var totalCount = await query.CountAsync();

            var itemsData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = itemsData.Select(tp => new PendingTutorResult
            {
                UserId = tp.UserId,
                FullName = tp.User.FullName,
                Email = tp.User.Email,
                Bio = tp.Bio,
                Qualifications = tp.Qualifications,
                CreatedAt = tp.CreatedAt
            }).ToList();

            return new PagedResult<PendingTutorResult>(items, totalCount);
        }

        public async Task<bool> RejectTutorAsync(Guid tutorUserId)
        {
            var profile = await _dbContext.TutorProfiles.FirstOrDefaultAsync(tp => tp.UserId == tutorUserId);
            if (profile == null) return false;

            profile.IsApproved = false;
            profile.ApprovalStatus = 2; // Rejected
            profile.ApprovedAt = null;
            profile.ApprovedBy = null;
            profile.UpdatedAt = DateTime.UtcNow;

            _dbContext.TutorProfiles.Update(profile);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendTutorApprovalNotificationAsync(Guid tutorUserId, bool isApproved)
        {
            var title = isApproved ? "Hồ sơ của bạn đã được duyệt!" : "Hồ sơ của bạn không được phê duyệt";
            var message = isApproved 
                ? "Chúc mừng bạn! Hồ sơ gia sư của bạn đã được phê duyệt thành công. Bây giờ bạn có thể thiết lập lịch dạy và bắt đầu nhận học viên." 
                : "Hồ sơ của bạn không được phê duyệt. Vui lòng cập nhật thông tin giới thiệu bản thân hoặc bằng cấp và gửi duyệt lại.";

            var notification = new NotificationDataModel
            {
                Id = Guid.NewGuid(),
                UserId = tutorUserId,
                Title = title,
                Message = message,
                Type = isApproved ? 5 : 7, // 5 = TutorApproved, 7 = System
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private string MapBookingStatusString(int status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Confirmed",
                2 => "Completed",
                3 => "Cancelled",
                4 => "Rescheduled",
                _ => "Unknown"
            };
        }

        public async Task<PagedResult<AdminUserResult>> GetAllUsersAsync(int pageNumber, int pageSize, string? search, int? role, bool? isActive)
        {
            // Filter: exclude Admin (0) unless specified otherwise? We said keep admin hidden.
            var query = _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Role != 0);

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(searchLower) ||
                                         u.Email.ToLower().Contains(searchLower));
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserResult
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreditBalance = u.CreditBalance,
                    CreatedAt = u.CreatedAt,
                    AdminNote = u.AdminNote,
                    AverageRating = u.TutorProfile != null ? u.TutorProfile.AverageRating : 0,
                    TotalReviews = u.TutorProfile != null ? u.TutorProfile.TotalReviews : 0
                })
                .ToListAsync();

            return new PagedResult<AdminUserResult>(items, totalCount);
        }

        public async Task<bool> LockUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || user.Role == 0) return false; // Cannot lock Admin accounts

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || user.Role == 0) return false; // Cannot unlock Admin accounts

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserNoteAsync(Guid userId, string? note)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || user.Role == 0) return false;

            user.AdminNote = note;
            user.UpdatedAt = DateTime.UtcNow;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<AdminReviewResult>> GetAllReviewsAsync(int pageNumber, int pageSize, string? search, int? reviewType, int? rating)
        {
            var query = _dbContext.Reviews
                .AsNoTracking()
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Tutor)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Student)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Subject)
                .AsQueryable();

            if (reviewType.HasValue)
            {
                query = query.Where(r => r.ReviewType == reviewType.Value);
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTrimmed = search.Trim();
                var searchLower = searchTrimmed.ToLower();

                if (_dbContext.Database.IsSqlServer())
                {
                    query = query.Where(r => 
                        (r.ReviewType == 0 && EF.Functions.Collate(r.Booking.Student.FullName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchTrimmed)) || 
                        (r.ReviewType == 1 && EF.Functions.Collate(r.Booking.Tutor.FullName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchTrimmed)) ||
                        (r.Comment != null && EF.Functions.Collate(r.Comment, "SQL_Latin1_General_CP1_CI_AI").Contains(searchTrimmed)));
                }
                else
                {
                    query = query.Where(r => 
                        (r.ReviewType == 0 && r.Booking.Student.FullName.ToLower().Contains(searchLower)) || 
                        (r.ReviewType == 1 && r.Booking.Tutor.FullName.ToLower().Contains(searchLower)) ||
                        (r.Comment != null && r.Comment.ToLower().Contains(searchLower)));
                }
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminReviewResult
                {
                    Id = r.Id,
                    ReviewerName = r.ReviewType == 0 ? r.Booking.Student.FullName : r.Booking.Tutor.FullName,
                    RevieweeName = r.ReviewType == 0 ? r.Booking.Tutor.FullName : r.Booking.Student.FullName,
                    SubjectName = r.Booking.Subject.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewType = r.ReviewType,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<AdminReviewResult>(items, totalCount);
        }

        private double CalculateGrowth(double current, double previous)
        {
            if (previous <= 0)
            {
                return current > 0 ? 100.0 : 0.0;
            }
            return Math.Round(((current - previous) / previous) * 100.0, 1);
        }
    }
}
