using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class TutorProfileConfiguration : IEntityTypeConfiguration<TutorProfile>
    {
        public void Configure(EntityTypeBuilder<TutorProfile> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.HourlyRate)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Qualifications)
                .HasMaxLength(500);

            // Relationships
            builder.HasMany(t => t.Sessions)
                .WithOne(s => s.Tutor)
                .HasForeignKey(s => s.TutorId)
                .OnDelete(DeleteBehavior.Restrict); 
            // Using Restrict to prevent multiple cascade paths in SQL server
        }
    }
}
