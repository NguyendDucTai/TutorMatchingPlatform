using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class GoalProgressConfiguration : IEntityTypeConfiguration<GoalProgressDataModel>
    {
        public void Configure(EntityTypeBuilder<GoalProgressDataModel> builder)
        {
            builder.ToTable("GoalProgress");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                   .HasMaxLength(1000);

            builder.HasIndex(x => x.LearningGoalId);
            builder.HasIndex(x => x.RecordedAt);

            builder.HasOne(x => x.LearningGoal)
                   .WithMany(x => x.GoalProgresses)
                   .HasForeignKey(x => x.LearningGoalId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
