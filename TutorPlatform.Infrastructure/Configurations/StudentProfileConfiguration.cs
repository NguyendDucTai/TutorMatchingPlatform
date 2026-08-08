using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfileDataModel>
    {
        public void Configure(EntityTypeBuilder<StudentProfileDataModel> builder)
        {
            builder.ToTable("StudentProfiles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GradeLevel)
                   .HasMaxLength(50);

            builder.Property(x => x.LearningPreferences)
                   .HasMaxLength(1000);

            builder.Property(x => x.AverageRating)
                   .HasColumnType("decimal(3,2)")
                   .HasDefaultValue(0);

            builder.HasIndex(x => x.UserId).IsUnique();

            builder.HasOne(x => x.User)
                   .WithOne(x => x.StudentProfile)
                   .HasForeignKey<StudentProfileDataModel>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
