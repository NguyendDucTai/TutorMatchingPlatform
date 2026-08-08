using System;
using System.Threading.Tasks;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Interfaces
{
    public interface ITutorSearchRepository
    {
        Task<PagedResult<TutorSearchResult>> SearchTutorsAsync(
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
            decimal? minRating = null);

        Task<bool> ApproveTutorAsync(Guid tutorUserId, Guid adminUserId);
        
        Task<TutorSearchResult?> GetTutorDetailAsync(Guid tutorUserId);
    }
}
