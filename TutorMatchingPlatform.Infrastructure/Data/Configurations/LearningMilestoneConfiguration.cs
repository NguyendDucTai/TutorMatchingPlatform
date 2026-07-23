using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class LearningMilestoneConfiguration : IEntityTypeConfiguration<LearningMilestone>
    {
        public void Configure(EntityTypeBuilder<LearningMilestone> builder)
        {
            builder.HasKey(lm => lm.Id);

            builder.Property(lm => lm.MilestoneName)
                .IsRequired()
                .HasMaxLength(200);

            // Relationships are already configured in StudentProfile and Subject, 
            // but we can ensure everything matches.
        }
    }
}
