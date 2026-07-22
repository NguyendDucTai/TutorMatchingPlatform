using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class SessionChangeRequestConfiguration : IEntityTypeConfiguration<SessionChangeRequest>
    {
        public void Configure(EntityTypeBuilder<SessionChangeRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ChangeType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Relationships
            builder.HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
