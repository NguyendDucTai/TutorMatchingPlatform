using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.MeetingLink)
                .HasMaxLength(500);

            builder.Property(s => s.TutorComment)
                .HasMaxLength(1000);

            // Relationships
            builder.HasMany(s => s.Feedbacks)
                .WithOne(f => f.Session)
                .HasForeignKey(f => f.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
