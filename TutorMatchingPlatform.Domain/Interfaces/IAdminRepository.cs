using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<PagedResult<PendingTutorResult>> GetPendingTutorsAsync(int pageNumber, int pageSize);
        Task<bool> RejectTutorAsync(Guid tutorUserId);
        Task<bool> SendTutorApprovalNotificationAsync(Guid tutorUserId, bool isApproved);

        // User Management
        Task<PagedResult<AdminUserResult>> GetAllUsersAsync(int pageNumber, int pageSize, string? search, int? role, bool? isActive);
        Task<bool> LockUserAsync(Guid userId);
        Task<bool> UnlockUserAsync(Guid userId);
        Task<bool> UpdateUserNoteAsync(Guid userId, string? note);

        // Review Management
        Task<PagedResult<AdminReviewResult>> GetAllReviewsAsync(int pageNumber, int pageSize, string? search, int? reviewType, int? rating);
    }
}
