using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Services;

namespace TutorMatchingPlatform.Application.Tutors.Queries.SearchTutors
{
    public class SearchTutorsQueryHandler : IRequestHandler<SearchTutorsQuery, SearchTutorsResult>
    {
        private readonly IAppDbContext _context;

        public SearchTutorsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SearchTutorsResult> Handle(SearchTutorsQuery request, CancellationToken cancellationToken)
        {
            var subjectSearch = $"\"SubjectId\":{request.SubjectId}";

            var approvedTutors = await _context.Users
                .Include(u => u.TutorProfile)
                .Include(u => u.TutorProfile!.FeedbacksReceived)
                .Where(u =>
                    u.Role == UserRole.Tutor &&
                    u.TutorProfile != null &&
                    u.TutorProfile.Status == ProfileStatus.Approved &&
                    u.TutorProfile.SubjectsJson != null &&
                    u.TutorProfile.SubjectsJson.Replace(" ", "").Contains(subjectSearch))
                .ToListAsync(cancellationToken);

            if (request.MinRate.HasValue || request.MaxRate.HasValue)
            {
                approvedTutors = approvedTutors.Where(u =>
                {
                    try
                    {
                        var subjects = JsonSerializer.Deserialize<List<SubjectRateEntry>>(u.TutorProfile!.SubjectsJson ?? "[]") ?? new();
                        var match = subjects.FirstOrDefault(s => s.SubjectId == request.SubjectId);
                        if (match == null) return false;
                        if (request.MinRate.HasValue && match.Rate < request.MinRate.Value) return false;
                        if (request.MaxRate.HasValue && match.Rate > request.MaxRate.Value) return false;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();
            }

            // Step 3: Calculate Reputation Score and Overlap
            var studentSlots = ParseSchedule(request.StudentScheduleJson);

            var results = approvedTutors.Select(u =>
            {
                double reputationScore = u.TutorProfile!.ReputationScore;

                int overlapMinutes = 0;
                if (studentSlots.Count > 0)
                {
                    var tutorSlots = ParseSchedule(u.TutorProfile.FreeSchedulesJson);
                    overlapMinutes = AvailabilityMatcher.CalculateOverlapMinutes(tutorSlots, studentSlots);
                }

                double overlapScore = System.Math.Min(overlapMinutes / 120.0, 5.0);
                double rankingScore = (overlapScore * 0.5) + (reputationScore * 0.5);

                return new TutorSearchResultDto
                {
                    TutorProfileId = u.TutorProfile.Id,
                    UserId = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.TutorProfile.Bio,
                    SubjectsJson = u.TutorProfile.SubjectsJson,
                    ReputationScore = System.Math.Round(reputationScore, 2),
                    OverlapMinutes = overlapMinutes,
                    RankingScore = System.Math.Round(rankingScore, 2)
                };
            })

            .Where(t => studentSlots.Count == 0 || t.OverlapMinutes > 0)
            .OrderByDescending(t => t.RankingScore)
            .ThenByDescending(t => t.OverlapMinutes)
            .ThenByDescending(t => t.ReputationScore)
            .ToList();

            if (results.Count == 0)
            {
                return new SearchTutorsResult
                {
                    HasResults = false,
                    Message = "No matching results found.",
                    Tutors = new(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            var totalCount = results.Count;
            var paged = results
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new SearchTutorsResult
            {
                HasResults = true,
                Tutors = paged,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private class SubjectRateEntry
        {
            public int SubjectId { get; set; }
            public decimal Rate { get; set; }
        }

        private List<TimeSlot> ParseSchedule(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<TimeSlot>();

            try
            {
                return JsonSerializer.Deserialize<List<TimeSlot>>(json) ?? new List<TimeSlot>();
            }
            catch
            {
                return new List<TimeSlot>();
            }
        }
    }
}
