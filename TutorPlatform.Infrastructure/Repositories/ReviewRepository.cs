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
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReviewRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            var dm = await _dbContext.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (dm == null) return null;

            return MapToDomain(dm);
        }

        public async Task<Review?> GetByBookingAndTypeAsync(Guid bookingId, ReviewType type)
        {
            var dm = await _dbContext.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.ReviewType == (int)type);

            if (dm == null) return null;

            return MapToDomain(dm);
        }

        public async Task<PagedResult<Review>> GetByRevieweeIdAsync(Guid revieweeId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Reviews
                .Where(r => r.RevieweeId == revieweeId)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var domainItems = items.Select(MapToDomain).ToList();

            return new PagedResult<Review>(domainItems, totalCount);
        }

        public async Task<Review> AddAsync(Review review)
        {
            var dm = new ReviewDataModel
            {
                Id = review.Id,
                BookingId = review.BookingId,
                ReviewerId = review.ReviewerId,
                RevieweeId = review.RevieweeId,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewType = (int)review.ReviewType,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };

            await _dbContext.Reviews.AddAsync(dm);
            await _dbContext.SaveChangesAsync();

            return review;
        }

        public async Task DeleteAsync(Review review)
        {
            var dm = await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            if (dm != null)
            {
                _dbContext.Reviews.Remove(dm);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RecalculateAverageRatingAsync(Guid revieweeId, bool isTutor)
        {
            var ratings = await _dbContext.Reviews
                .Where(r => r.RevieweeId == revieweeId)
                .Select(r => r.Rating)
                .ToListAsync();

            decimal avgRating = ratings.Any() ? (decimal)ratings.Average() : 0;
            int totalReviews = ratings.Count;

            if (isTutor)
            {
                var profile = await _dbContext.TutorProfiles
                    .FirstOrDefaultAsync(tp => tp.UserId == revieweeId);

                if (profile != null)
                {
                    // Use a backing field or reflection since AverageRating/TotalReviews properties have private setters.
                    var type = typeof(TutorProfileDataModel);
                    type.GetProperty("AverageRating")?.SetValue(profile, avgRating);
                    type.GetProperty("TotalReviews")?.SetValue(profile, totalReviews);
                    type.GetProperty("UpdatedAt")?.SetValue(profile, DateTime.UtcNow);

                    _dbContext.TutorProfiles.Update(profile);
                }
            }
            else
            {
                var profile = await _dbContext.StudentProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == revieweeId);

                if (profile != null)
                {
                    var type = typeof(StudentProfileDataModel);
                    type.GetProperty("AverageRating")?.SetValue(profile, avgRating);
                    type.GetProperty("UpdatedAt")?.SetValue(profile, DateTime.UtcNow);

                    _dbContext.StudentProfiles.Update(profile);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<PagedResult<(Review Review, string ReviewerName, string? ReviewerAvatar, string SubjectName)>> GetReviewsWithDetailsAsync(Guid revieweeId, int pageNumber, int pageSize)
        {
            var query = from r in _dbContext.Reviews
                        join u in _dbContext.Users on r.ReviewerId equals u.Id
                        join b in _dbContext.Bookings on r.BookingId equals b.Id
                        join s in _dbContext.Subjects on b.SubjectId equals s.Id
                        where r.RevieweeId == revieweeId
                        select new
                        {
                            Review = r,
                            ReviewerName = u.FullName,
                            ReviewerAvatar = u.AvatarUrl,
                            SubjectName = s.Name
                        };

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.Review.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultItems = items.Select(x =>
            {
                var domainReview = MapToDomain(x.Review);
                return (domainReview, x.ReviewerName, (string?)x.ReviewerAvatar, x.SubjectName);
            }).ToList();

            return new PagedResult<(Review, string, string?, string)>(resultItems, totalCount);
        }

        private Review MapToDomain(ReviewDataModel dm)
        {
            var review = new Review(
                dm.BookingId,
                dm.ReviewerId,
                dm.RevieweeId,
                dm.Rating,
                dm.Comment,
                (ReviewType)dm.ReviewType
            );

            var type = typeof(Review);
            type.GetProperty("Id")?.SetValue(review, dm.Id);
            type.GetProperty("CreatedAt")?.SetValue(review, dm.CreatedAt);
            type.GetProperty("UpdatedAt")?.SetValue(review, dm.UpdatedAt);

            return review;
        }
    }
}
