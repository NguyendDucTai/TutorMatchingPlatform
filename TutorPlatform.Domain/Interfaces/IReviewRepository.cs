using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<Review?> GetByBookingAndTypeAsync(Guid bookingId, Enums.ReviewType type);
        Task<PagedResult<Review>> GetByRevieweeIdAsync(Guid revieweeId, int pageNumber, int pageSize);
        Task<Review> AddAsync(Review review);
        Task RecalculateAverageRatingAsync(Guid revieweeId, bool isTutor);
        Task<PagedResult<(Review Review, string ReviewerName, string? ReviewerAvatar, string SubjectName)>> GetReviewsWithDetailsAsync(Guid revieweeId, int pageNumber, int pageSize);
        Task DeleteAsync(Review review);
    }
}
