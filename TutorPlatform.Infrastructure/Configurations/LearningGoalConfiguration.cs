using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class LearningGoalConfiguration : IEntityTypeConfiguration<LearningGoalDataModel>
    {
        public void Configure(EntityTypeBuilder<LearningGoalDataModel> builder)
        {
            builder.ToTable("LearningGoals");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(x => x.Description)
                   .HasMaxLength(2000);

            builder.Property(x => x.TargetDate)
                   .HasColumnType("date");

            builder.HasIndex(x => new { x.TutorId, x.StudentId });
            builder.HasIndex(x => new { x.StudentId, x.SubjectId });
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Tutor)
                   .WithMany()
                   .HasForeignKey(x => x.TutorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Student)
                   .WithMany()
                   .HasForeignKey(x => x.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Subject)
                   .WithMany()
                   .HasForeignKey(x => x.SubjectId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
