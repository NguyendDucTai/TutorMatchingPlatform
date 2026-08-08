using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface ILearningGoalRepository
    {
        Task<LearningGoal?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<LearningGoal>> GetByStudentIdAsync(Guid studentId);
        Task<IReadOnlyList<LearningGoal>> GetByTutorAndStudentAsync(Guid tutorId, Guid studentId);
        Task<LearningGoal> AddAsync(LearningGoal goal);
        Task UpdateAsync(LearningGoal goal);
    }
}
