using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            // Relationships
            builder.HasMany(s => s.Sessions)
                .WithOne(session => session.Subject)
                .HasForeignKey(session => session.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasMany(s => s.LearningMilestones)
                .WithOne(lm => lm.Subject)
                .HasForeignKey(lm => lm.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
