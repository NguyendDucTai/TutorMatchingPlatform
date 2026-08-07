using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Infrastructure.Models;

namespace TutorMatchingPlatform.Infrastructure.Configurations
{
    public class TutorSubjectConfiguration : IEntityTypeConfiguration<TutorSubjectDataModel>
    {
        public void Configure(EntityTypeBuilder<TutorSubjectDataModel> builder)
        {
            builder.ToTable("TutorSubjects");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.TutorProfileId, x.SubjectId }).IsUnique();

            builder.Property(x => x.HourlyCredits)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.HasOne(x => x.TutorProfile)
                   .WithMany(x => x.TutorSubjects)
                   .HasForeignKey(x => x.TutorProfileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Subject)
                   .WithMany(x => x.TutorSubjects)
                   .HasForeignKey(x => x.SubjectId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
