using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<TutorProfile> TutorProfiles { get; }
        DbSet<StudentProfile> StudentProfiles { get; }
        DbSet<Subject> Subjects { get; }
        DbSet<Session> Sessions { get; }
        DbSet<LearningMilestone> LearningMilestones { get; }
        DbSet<Feedback> Feedbacks { get; }
        DbSet<Notification> Notifications { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
