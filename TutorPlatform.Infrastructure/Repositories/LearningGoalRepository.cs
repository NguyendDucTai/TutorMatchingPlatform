using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class LearningGoalRepository : ILearningGoalRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public LearningGoalRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LearningGoal?> GetByIdAsync(Guid id)
        {
            var dm = await _dbContext.LearningGoals
                .Include(lg => lg.GoalProgresses)
                .AsNoTracking()
                .FirstOrDefaultAsync(lg => lg.Id == id);

            if (dm == null) return null;

            return MapToDomain(dm);
        }

        public async Task<IReadOnlyList<LearningGoal>> GetByStudentIdAsync(Guid studentId)
        {
            var dataModels = await _dbContext.LearningGoals
                .Include(lg => lg.GoalProgresses)
                .AsNoTracking()
                .Where(lg => lg.StudentId == studentId)
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<IReadOnlyList<LearningGoal>> GetByTutorAndStudentAsync(Guid tutorId, Guid studentId)
        {
            var dataModels = await _dbContext.LearningGoals
                .Include(lg => lg.GoalProgresses)
                .AsNoTracking()
                .Where(lg => lg.TutorId == tutorId && lg.StudentId == studentId)
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<LearningGoal> AddAsync(LearningGoal goal)
        {
            var dm = new LearningGoalDataModel
            {
                Id = goal.Id,
                TutorId = goal.TutorId,
                StudentId = goal.StudentId,
                SubjectId = goal.SubjectId,
                Title = goal.Title,
                Description = goal.Description,
                TargetDate = goal.TargetDate,
                Status = (int)goal.Status,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            };

            await _dbContext.LearningGoals.AddAsync(dm);
            await _dbContext.SaveChangesAsync();

            return goal;
        }

        public async Task UpdateAsync(LearningGoal goal)
        {
            var dm = await _dbContext.LearningGoals
                .Include(lg => lg.GoalProgresses)
                .FirstOrDefaultAsync(lg => lg.Id == goal.Id);

            if (dm != null)
            {
                dm.Title = goal.Title;
                dm.Description = goal.Description;
                dm.TargetDate = goal.TargetDate;
                dm.Status = (int)goal.Status;
                dm.UpdatedAt = DateTime.UtcNow;

                // Sync GoalProgresses: Insert new records directly (optimizing EF memory usage)
                var existingGpIds = dm.GoalProgresses.Select(gp => gp.Id).ToHashSet();
                foreach (var domainGp in goal.GoalProgresses)
                {
                    if (!existingGpIds.Contains(domainGp.Id))
                    {
                        var newGpDm = new GoalProgressDataModel
                        {
                            Id = domainGp.Id == Guid.Empty ? Guid.NewGuid() : domainGp.Id,
                            LearningGoalId = domainGp.LearningGoalId,
                            ProgressPercentage = domainGp.ProgressPercentage,
                            Notes = domainGp.Notes,
                            RecordedAt = domainGp.RecordedAt,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _dbContext.GoalProgress.AddAsync(newGpDm);
                    }
                }

                _dbContext.LearningGoals.Update(dm);
                await _dbContext.SaveChangesAsync();
            }
        }

        private LearningGoal MapToDomain(LearningGoalDataModel dm)
        {
            var goal = new LearningGoal(dm.TutorId, dm.StudentId, dm.SubjectId, dm.Title, dm.Description, dm.TargetDate);

            var type = typeof(LearningGoal);
            type.GetProperty("Id")?.SetValue(goal, dm.Id);
            
            // Map Status property (which has private setter)
            type.GetProperty("Status")?.SetValue(goal, (GoalStatus)dm.Status);

            // Populate the private readonly List<GoalProgress> _goalProgresses list via reflection
            var listField = type.GetField("_goalProgresses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = (List<GoalProgress>)listField?.GetValue(goal)!;

            if (dm.GoalProgresses != null)
            {
                foreach (var gpData in dm.GoalProgresses.OrderBy(gp => gp.RecordedAt))
                {
                    var progress = new GoalProgress(gpData.LearningGoalId, gpData.ProgressPercentage, gpData.Notes);
                    var gpType = typeof(GoalProgress);
                    gpType.GetProperty("Id")?.SetValue(progress, gpData.Id);
                    gpType.GetProperty("RecordedAt")?.SetValue(progress, gpData.RecordedAt);
                    list.Add(progress);
                }
            }

            return goal;
        }
    }
}
