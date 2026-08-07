using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Infrastructure.Models;

namespace TutorMatchingPlatform.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserDataModel> Users { get; set; } = null!;
        public DbSet<TutorProfileDataModel> TutorProfiles { get; set; } = null!;
        public DbSet<StudentProfileDataModel> StudentProfiles { get; set; } = null!;
        public DbSet<SubjectDataModel> Subjects { get; set; } = null!;
        public DbSet<TutorSubjectDataModel> TutorSubjects { get; set; } = null!;
        public DbSet<AvailabilityDataModel> Availabilities { get; set; } = null!;
        public DbSet<BookingDataModel> Bookings { get; set; } = null!;
        public DbSet<SessionRecordDataModel> SessionRecords { get; set; } = null!;
        public DbSet<LearningGoalDataModel> LearningGoals { get; set; } = null!;
        public DbSet<GoalProgressDataModel> GoalProgress { get; set; } = null!;
        public DbSet<ReviewDataModel> Reviews { get; set; } = null!;
        public DbSet<CreditTransactionDataModel> CreditTransactions { get; set; } = null!;
        public DbSet<NotificationDataModel> Notifications { get; set; } = null!;
        public DbSet<DepositRequestDataModel> DepositRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
