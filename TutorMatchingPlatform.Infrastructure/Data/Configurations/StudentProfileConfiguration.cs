using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
    {
        public void Configure(EntityTypeBuilder<StudentProfile> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StudyGoals)
                .HasMaxLength(1000);

            // Relationships
            builder.HasMany(s => s.Sessions)
                .WithOne(session => session.Student)
                .HasForeignKey(session => session.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.LearningMilestones)
                .WithOne(lm => lm.Student)
                .HasForeignKey(lm => lm.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
